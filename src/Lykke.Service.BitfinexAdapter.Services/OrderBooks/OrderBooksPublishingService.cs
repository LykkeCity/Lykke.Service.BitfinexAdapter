using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.ExchangeAdapter;
using Lykke.Common.ExchangeAdapter.Contracts;
using Lykke.Common.ExchangeAdapter.Tools.ObservableWebSocket;
using Lykke.Service.BitfinexAdapter.Core.Domain.Settings;
using Lykke.Service.BitfinexAdapter.Core.Domain.WebSocketClient;
using Lykke.Service.BitfinexAdapter.Core.RestClient;
using Microsoft.Extensions.Hosting;

namespace Lykke.Service.BitfinexAdapter.Services.OrderBooks
{
    public sealed class OrderBooksPublishingService : IHostedService
    {
        private readonly ILog _log;
        private readonly BitfinexAdapterSettings _settings;
        private IDisposable _worker;

        private static readonly TimeSpan PingsInterval = TimeSpan.FromSeconds(5);

        public OrderBooksPublishingService(ILog log, BitfinexAdapterSettings settings)
        {
            _log = log;
            _settings = settings;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _worker = CreateWorker()
                .ReportErrors(nameof(OrderBooksPublishingService), _log)
                .RetryWithBackoff(TimeSpan.FromSeconds(5), TimeSpan.FromMinutes(10))
                .Subscribe(_ => { },
                    err => _log.WriteErrorAsync(nameof(OrderBooksPublishingService), "", err));

            return Task.CompletedTask;
        }

        private IObservable<Unit> CreateWorker()
        {
            var webSocket = new ObservableWebSocket(
                    "wss://api.bitfinex.com/ws",
                    info => _log.WriteInfo(nameof(ObservableWebSocket), "", info))
                .ReportErrors(nameof(ObservableWebSocket), _log)
                .RetryWithBackoff(TimeSpan.FromSeconds(0.5), TimeSpan.FromMinutes(10))
                .Share();

            return ReadAndPublish(webSocket);
        }

        private IObservable<Unit> ReadAndPublish(IObservable<ISocketEvent> webSocket)
        {
            var statWindow = TimeSpan.FromMinutes(1);

            return Observable.Defer(() =>
            {
                var messageReader = new BitfinexOrderBookWebSocketReader(_log);

                var sendPings = webSocket
                    .Throttle(PingsInterval)
                    .SelectMany(async x =>
                    {
                        await x.Session.SendAsJson(new PingRequest());
                        return (ISocketEvent) null;
                    });

                var orderBooks =
                    Observable.Merge(
                            webSocket,
                            SubscribeOnConnect(webSocket),
                            sendPings)
                        .Where(x => x != null)
                        .Select(messageReader.DeserializeMessage)
                        .Where(x => x != null)
                        .OnlyWithPositiveSpread()
                        .DetectAndFilterAnomalies(_log, _settings.AllowedAnomalisticOrderBooksAssets)
                        .Share();

                var obPublisher =
                    orderBooks
                        .ThrottleEachInstrument(x => x.Asset, _settings.MaxEventPerSecondByInstrument)
                        .PublishToRmq(
                            _settings.RabbitMq.OrderBooks.ConnectionString,
                            _settings.RabbitMq.OrderBooks.Exchange,
                            _log)
                        .ReportErrors(nameof(ReadAndPublish), _log)
                        .RetryWithBackoff(TimeSpan.FromSeconds(5), TimeSpan.FromMinutes(10))
                        .Share();

                var tpPublisher =
                    orderBooks
                        .Select(TickPrice.FromOrderBook)
                        .DistinctEveryInstrument(x => x.Asset)
                        .ThrottleEachInstrument(x => x.Asset, _settings.MaxEventPerSecondByInstrument)
                        .PublishToRmq(
                            _settings.RabbitMq.TickPrices.ConnectionString,
                            _settings.RabbitMq.TickPrices.Exchange,
                            _log)
                        .ReportErrors(nameof(ReadAndPublish), _log)
                        .RetryWithBackoff(TimeSpan.FromSeconds(5), TimeSpan.FromMinutes(10))
                        .Share();

                var publishTickPrices = _settings.RabbitMq.TickPrices.Enabled;
                var publishOrderBooks = _settings.RabbitMq.OrderBooks.Enabled;

                return Observable.Merge(
                    tpPublisher.NeverIfNotEnabled(publishTickPrices),
                    obPublisher.NeverIfNotEnabled(publishOrderBooks),

                    orderBooks.ReportStatistics(
                            statWindow,
                            _log,
                            "OrderBooks received from WebSocket in the last {0}: {1}")
                        .NeverIfNotEnabled(_settings.RabbitMq.TickPrices.Enabled ||
                                           _settings.RabbitMq.OrderBooks.Enabled),

                    tpPublisher.ReportStatistics(statWindow, _log, "TickPrices published in the last {0}: {1}")
                        .NeverIfNotEnabled(publishTickPrices),

                    obPublisher.ReportStatistics(statWindow, _log, "OrderBooks published in the last {0}: {1}")
                        .NeverIfNotEnabled(publishOrderBooks)
                );
            });
        }

        private IObservable<ISocketEvent> SubscribeOnConnect(IObservable<ISocketEvent> webSocket)
        {
            return webSocket
                .Where(x => x is SocketConnected)
                .SelectMany(async s =>
                {
                    var pairs = (await GetPairsToSubscribe()).Distinct();

                    foreach (var p in pairs)
                    {
                        await s.Session.SendAsJson(new SubscribeOrderBooksRequest
                        {
                            Event = "subscribe",
                            Pair = p,
                            Channel = WsChannel.book,
                            Freq = "F0",
                            Prec = "P0",
                            Length = 100
                        });

                        await Task.Delay(TimeSpan.FromMilliseconds(50));
                    }

                    return (ISocketEvent)null;
                });
        }

        private async Task<string[]> GetPairsToSubscribe()
        {
            var supported = _settings.SupportedCurrencySymbols.Select(x => x.ExchangeSymbol.ToUpper()).ToArray();

            if (_settings.UseSupportedCurrencySymbolsAsFilter)
            {
                return supported;
            }
            else
            {
                var bitfinexServiceClientCredentials =
                    new BitfinexServiceClientCredentials(
                        string.Empty,
                        string.Empty); // bitfinex does not require key/secret for public events

                var api = new BitfinexApi(bitfinexServiceClientCredentials, _log)
                {
                    BaseUri = new Uri(_settings.EndpointUrl)
                };

                return (await api.GetAllSymbolsAsync())
                    .Select(x => x.ToUpper())
                    .Concat(supported).ToArray();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _worker?.Dispose();
            return Task.CompletedTask;
        }
    }
}
