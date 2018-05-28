using Common;
using Common.Log;
using Lykke.Service.BitfinexAdapter.Core.Domain.OrderBooks;
using Lykke.Service.BitfinexAdapter.Core.Domain.Settings;
using Lykke.Service.BitfinexAdapter.Core.Domain.Trading;
using Lykke.Service.BitfinexAdapter.Core.Domain.WebSocketClient;
using Lykke.Service.BitfinexAdapter.Core.Handlers;
using Lykke.Service.BitfinexAdapter.Core.RestClient;
using Lykke.Service.BitfinexAdapter.Core.Throttling;
using Lykke.Service.BitfinexAdapter.Core.Utils;
using Lykke.Service.BitfinexAdapter.Core.WebSocketClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Common.ExchangeAdapter.Contracts;
using TickPrice = Lykke.Service.BitfinexAdapter.Core.Domain.Trading.TickPrice;

namespace Lykke.Service.BitfinexAdapter.Services.OrderBooksHarvester
{
    public sealed class BitfinexOrderBooksHarvester : OrderBooksWebSocketHarvester<object, string>, IStopable
    {
        private readonly BitfinexAdapterSettings _configuration;
        private readonly Dictionary<long, Channel> _channels;
        private readonly IHandler<TickPrice> _tickPriceHandler;
        private readonly IThrottling _tickPriceThrottler;
        private readonly IBitfinexApi _exchangeApi;

        public BitfinexOrderBooksHarvester(BitfinexAdapterSettings configuration,
            IHandler<OrderBook> orderBookHandler,
            IHandler<TickPrice> tickPriceHandler,
            IThrottling orderBooksThrottler,
            IThrottling tickPriceThrottler,
            ILog log)
        : base(configuration, new WebSocketTextMessenger(configuration.WebSocketEndpointUrl, log), log, orderBookHandler, orderBooksThrottler)
        {
            _configuration = configuration;
            _channels = new Dictionary<long, Channel>();
            _tickPriceHandler = tickPriceHandler;
            var credenitals = new BitfinexServiceClientCredentials(String.Empty, String.Empty); // bitfinex does not require key/scret for public events
            _exchangeApi = new BitfinexApi(credenitals, log)
            {
                BaseUri = new Uri(configuration.EndpointUrl)
            };
            _tickPriceThrottler = tickPriceThrottler;
        }


        protected override async Task MessageLoopImpl()
        {
            if (!_configuration.RabbitMq.TickPrices.Enabled && !_configuration.RabbitMq.OrderBooks.Enabled)
            {
                return;
            }

            try
            {
                await Messenger.ConnectAsync(CancellationToken);
                await Subscribe();
                RechargeHeartbeat();
                while (!CancellationToken.IsCancellationRequested)
                {
                    var resp = await GetResponse();
                    RechargeHeartbeat();
                    await HandleResponse(resp);
                }
            }
            finally
            {
                try
                {
                    await Messenger.StopAsync(CancellationToken);
                }
                catch
                {
                    // Nothing to do here
                }
            }
        }

        private async Task<dynamic> GetResponse()
        {
            var json = await Messenger.GetResponseAsync(CancellationToken);

            var result = EventResponse.Parse(json) ??
                TickerResponse.Parse(json) ??
                OrderBookSnapshotResponse.Parse(json) ??
                (dynamic)OrderBookUpdateResponse.Parse(json) ??
                HeartbeatResponse.Parse(json);
            return result;
        }

        private async Task Subscribe()
        {
            var instrumentsToSubscribeFor = _configuration.SupportedCurrencySymbols?
                                  .Select(s => s.ExchangeSymbol)
                                  .ToList() ?? new List<string>();

            if (_configuration.UseSupportedCurrencySymbolsAsFilter == false)
            {
                var response = await _exchangeApi.GetAllSymbolsAsync(CancellationToken);
                var instrumentsFromExchange = (response).Select(i => i.ToUpper()).ToList();

                foreach (var exchInstr in instrumentsFromExchange)
                {
                    if (!instrumentsToSubscribeFor.Contains(exchInstr))
                    {
                        instrumentsToSubscribeFor.Add(exchInstr);
                    }
                }
            }

            if (!instrumentsToSubscribeFor.Any())
            {
                await Log.WriteWarningAsync(nameof(Subscribe), "Subscribing for orderbooks", "Instruments list is empty - its either not set in config and UseSupportedCurrencySymbolsAsFilter is set to true or exchange returned empty symbols list. No symbols to subscribe for.");
            }

            if (_configuration.RabbitMq.OrderBooks.Enabled)
            {
                await SubscribeToOrderBookAsync(instrumentsToSubscribeFor);
            }
            if (_configuration.RabbitMq.TickPrices.Enabled)
            {
                await SubscribeToTickerAsync(instrumentsToSubscribeFor);
            }
        }

        private async Task SubscribeToOrderBookAsync(IEnumerable<string> instruments)
        {
            foreach (var instrument in instruments)
            {
                var request = SubscribeOrderBooksRequest.BuildRequest(
                    instrument,
                    "F0",
                    "R0",
                    SubscribeOrderBooksRequest.OrderBookLength.OneHundred);

                await Messenger.SendRequestAsync(request, CancellationToken);
                var response = await GetResponse();
                await HandleResponse(response);
            }
        }

        private async Task SubscribeToTickerAsync(IEnumerable<string> instruments)
        {
            foreach (var instrument in instruments)
            {
                var request = SublscribeTickeRequest.BuildRequest(instrument);
                await Messenger.SendRequestAsync(request, CancellationToken);
                var response = await GetResponse();
                await HandleResponse(response);
            }
        }

        private async Task HandleResponse(InfoResponse response)
        {
            await Log.WriteInfoAsync(nameof(HandleResponse), "Connecting to Bitfinex", $"{response.Event} Version {response.Version}");
        }

        private async Task HandleResponse(SubscribedResponse response)
        {
            await Log.WriteInfoAsync(nameof(HandleResponse), "Subscribing on the order book", $"Event: {response.Event} Channel: {response.Channel} Pair: {response.Pair}");

            if (!_channels.TryGetValue(response.ChanId, out var channel))
            {
                channel = new Channel(response.ChanId, response.Pair);
                _channels[channel.Id] = channel;
            }
        }

        private async Task HandleResponse(EventMessageResponse response)
        {
            await Log.WriteInfoAsync(nameof(HandleResponse), "Subscribed on the order book", $"Event: {response.Event} Code: {response.Code} Message: {response.Message}");
        }

        private Task HandleResponse(ErrorEventMessageResponse response)
        {
            throw new InvalidOperationException($"Event: {response.Event} Code: {response.Code} Message: {response.Message}");
        }

        private async Task HandleResponse(HeartbeatResponse heartbeat)
        {
            //await Log.WriteInfoAsync(nameof(HandleResponse), $"Bitfinex channel {_channels[heartbeat.ChannelId].Pair} heartbeat", string.Empty);
        }

        private async Task HandleResponse(OrderBookSnapshotResponse snapshot)
        {
            var pair = _channels[snapshot.ChannelId].Pair;
            var buysCount = snapshot.Orders.Count(x => x.Amount > 0);
            var sellsCount = snapshot.Orders.Count(x => x.Amount < 0);

            Log.WriteInfo(nameof(BitfinexOrderBooksHarvester), pair, $"Received orderbooksnapshot, " +
                                                                     $"buys: {buysCount}, " +
                                                                     $"sells: {sellsCount}");

            await HandleOrderBookSnapshotAsync(pair,
                DateTime.UtcNow, // TODO: Get this from the server
                snapshot.Orders.Select(BitfinexModelConverter.ToOrderBookItem));
        }

        private async Task HandleResponse(TickerResponse ticker)
        {
            var pair = _channels[ticker.ChannelId].Pair;

            if (_tickPriceThrottler.NeedThrottle(pair))
            {
                return;
            }

            var tickPrice = new TickPrice(new Instrument(pair), DateTime.UtcNow, 
                ticker.Ask, ticker.Bid);
            await CallTickPricesHandlers(tickPrice);
        }

        private async Task HandleResponse(OrderBookUpdateResponse response)
        {
            var orderBookItem = BitfinexModelConverter.ToOrderBookItem(response);
            var pair = _channels[response.ChannelId].Pair;
            response.Pair = pair;

            if (response.Price == 0)
            {
                await HandleOrdersEventsAsync(response.Pair,
                    OrderBookEventType.Delete, new[] { orderBookItem });
            }
            else
            {
                await HandleOrdersEventsAsync(response.Pair,
                    OrderBookEventType.Add, new[] { orderBookItem });
            }
        }

        private Task CallTickPricesHandlers(TickPrice tickPrice)
        {
            return _tickPriceHandler.Handle(tickPrice);
        }

        private sealed class Channel
        {
            public long Id { get; }
            public string Pair { get; }

            public Channel(long id, string pair)
            {
                Id = id;
                Pair = pair;
            }
        }
    }
}
