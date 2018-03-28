using Common.Log;
using Lykke.Service.BitfinexAdapter.Core.Domain;
using Lykke.Service.BitfinexAdapter.Core.Domain.Exceptions;
using Lykke.Service.BitfinexAdapter.Core.Domain.RestClient;
using Lykke.Service.BitfinexAdapter.Core.Domain.Settings;
using Lykke.Service.BitfinexAdapter.Core.Domain.Trading;
using Lykke.Service.BitfinexAdapter.Core.Domain.Trading.Enums;
using Lykke.Service.BitfinexAdapter.Core.RestClient;
using Lykke.Service.BitfinexAdapter.Core.Utils;
using Lykke.Service.BitfinexAdapter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

//using Position = Lykke.Service.BitfinexAdapter.Core.Domain.Trading.Position;

namespace Lykke.Service.BitfinexAdapter.Services.Exchange
{
    public class BitfinexExchange : ExchangeBase
    {
        private readonly BitfinexModelConverter _modelConverter;
        private readonly IBitfinexApi _exchangeApi;

        public BitfinexExchange(
            BitfinexAdapterSettings configuration,
            string apiKey,
            string secret,
            ILog log)
            : base(Constants.BitfinexExchangeName, configuration, log)
        {
            _modelConverter = new BitfinexModelConverter(configuration);
            var credenitals = new BitfinexServiceClientCredentials(apiKey, secret); //TODO: key/secret must come from config, after verifying client request X-API-KEY. We may need to create separe BitfinexExchange/BitfinexApi for each client. _orderBooksHarvester may need to be taken out from here and started separately.
            _exchangeApi = new BitfinexApi(credenitals)
            {
                BaseUri = new Uri(configuration.EndpointUrl)
            };
        }

        public override async Task<ExecutionReport> AddOrderAndWaitExecution(TradingSignal signal, TimeSpan timeout)
        {
            var symbol = _modelConverter.LykkeSymbolToExchangeSymbol(signal.Instrument.Name);
            var volume = signal.Volume;
            var orderType = _modelConverter.ConvertOrderType(signal.OrderType);
            var side = _modelConverter.ConvertTradeType(signal.TradeType);
            var price = signal.Price == 0 ? 1 : signal.Price ?? 1;
            var cts = new CancellationTokenSource(timeout);

            var response = await _exchangeApi.AddOrderAsync(symbol, volume, price, side, orderType, cts.Token);

            if (response is Error error)
            {
                throw new ApiException(error.Message);
            }

            var trade = OrderToTrade((Order)response);
            return trade;
        }

        public override async Task<ExecutionReport> CancelOrderAndWaitExecution(TradingSignal signal, TimeSpan timeout)
        {

            var cts = new CancellationTokenSource(timeout);
            if (!long.TryParse(signal.OrderId, out var id))
            {
                throw new ApiException("Bitfinex order id can be only integer");
            }
            var response = await _exchangeApi.CancelOrderAsync(id, cts.Token);
            if (response is Error error)
            {
                throw new ApiException(error.Message);
            }
            var trade = OrderToTrade((Order)response);
            return trade;
        }

        public override async Task<ExecutionReport> GetOrder(string id, Instrument instrument, TimeSpan timeout)
        {
            if (!long.TryParse(id, out var orderId))
            {
                throw new ApiException("Bitfinex order id can be only integer");
            }
            var cts = new CancellationTokenSource(timeout);
            var response = await _exchangeApi.GetOrderStatusAsync(orderId, cts.Token);
            if (response is Error error)
            {
                throw new ApiException(error.Message);
            }
            var trade = OrderToTrade((Order)response);
            return trade;
        }

        public override async Task<IEnumerable<ExecutionReport>> GetOpenOrders(TimeSpan timeout)
        {

            var cts = new CancellationTokenSource(timeout);
            var response = await _exchangeApi.GetActiveOrdersAsync(cts.Token);
            if (response is Error error)
            {
                throw new ApiException(error.Message);
            }
            var trades = ((IReadOnlyCollection<Order>)response).Select(OrderToTrade);
            return trades;
        }

        public override async Task<IReadOnlyCollection<TradingPosition>> GetPositionsAsync(TimeSpan timeout)
        {
            var cts = new CancellationTokenSource(timeout);
            var response = await _exchangeApi.GetActivePositionsAsync(cts.Token);
            if (response is Error error)
            {
                throw new ApiException(error.Message);
            }
            var marginInfo = await GetMarginInfo(timeout);
            var positions = ExchangePositionsToPositionModel((IReadOnlyCollection<Position>)response, marginInfo);
            return positions;
        }

        public override StreamingSupport StreamingSupport => new StreamingSupport(true, true);
        public override async Task<IReadOnlyList<string>> GetAllExchangeInstruments()
        {
            var response = await _exchangeApi.GetAllSymbolsAsync();
            if (response is Error error)
            {
                throw new ApiException(error.Message);
            }
            var instrumentsFromExchange = ((IReadOnlyList<string>)response).Select(i => i.ToUpper()).ToList();
            return instrumentsFromExchange;
        }

        private IReadOnlyCollection<TradingPosition> ExchangePositionsToPositionModel(IEnumerable<Position> response, IReadOnlyList<MarginInfo> marginInfo)
        {
            var marginByCurrency = marginInfo[0].MarginLimits.ToDictionary(ml => ml.OnPair, ml => ml, StringComparer.InvariantCultureIgnoreCase);
            var result = response.Select(r =>
                new TradingPosition
                {
                    Symbol = _modelConverter.ExchangeSymbolToLykkeInstrument(r.Symbol).Name,
                    PositionVolume = r.Amount,
                    MaintMarginUsed = r.Amount * r.Base * marginByCurrency[r.Symbol].MarginRequirement / 100m,
                    RealisedPnL = 0, //TODO no specification,
                    UnrealisedPnL = r.Pl,
                    PositionValue = r.Amount * r.Base,
                    AvailableMargin = Math.Max(0, marginByCurrency[r.Symbol].TradableBalance) * marginByCurrency[r.Symbol].InitialMargin / 100m,
                    InitialMarginRequirement = marginByCurrency[r.Symbol].InitialMargin / 100m,
                    MaintenanceMarginRequirement = marginByCurrency[r.Symbol].MarginRequirement / 100m
                }
            );
            return result.ToArray();
        }

        public override async Task<IReadOnlyCollection<MarginBalanceDomain>> GetMarginBalances(TimeSpan timeout)
        {
            var marginInfor = await GetMarginInfo(timeout);
            var result = MarginInfoToBalance(marginInfor);
            return result;
        }

        public override async Task<IReadOnlyCollection<WalletBalance>> GetWalletBalances(TimeSpan timeout)
        {
            var cts = new CancellationTokenSource(timeout);
            var balances = await _exchangeApi.GetWalletBalancesAsync(cts.Token);
            if (balances is Error error)
            {
                throw new ApiException(error.Message);
            }
            return (IReadOnlyList<WalletBalance>)balances;
        }


        private async Task<IReadOnlyList<MarginInfo>> GetMarginInfo(TimeSpan timeout)
        {
            var cts = new CancellationTokenSource(timeout);
            var response = await _exchangeApi.GetMarginInformationAsync(cts.Token);
            if (response is Error error)
            {
                throw new ApiException(error.Message);
            }
            var marginInfor = (IReadOnlyList<MarginInfo>)response;

            return marginInfor;
        }

        private static IReadOnlyCollection<MarginBalanceDomain> MarginInfoToBalance(IReadOnlyList<MarginInfo> marginInfos)
        {
            if (marginInfos.Count != 1)
            {
                throw new ApiException(@"Incorrect number of marginInfo. Expected 1 but received {marginInfo.Count}");
            }

            var mi = marginInfos[0];
            var balance = new MarginBalanceDomain
            {
                AccountCurrency = "USD",
                Totalbalance = mi.NetValue,
                UnrealisedPnL = mi.UnrealizedPl,
                MarginBalance = mi.MarginBalance,
                TradableBalance = mi.TradableBalance,
                MarginUsed = mi.RequiredMargin
            };

            return new[] { balance };
        }

        private ExecutionReport OrderToTrade(Order order)
        {
            var id = order.Id;
            var execTime = order.Timestamp;
            var execPrice = order.Price;
            var execVolume = order.ExecutedAmount;
            var tradeType = BitfinexModelConverter.ConvertTradeType(order.Side);
            var status = ConvertExecutionStatus(order);
            var instr = _modelConverter.ExchangeSymbolToLykkeInstrument(order.Symbol);

            return new ExecutionReport(instr, execTime, execPrice, execVolume, tradeType, id, status)
            {
                ExecType = ExecType.Trade,
                Success = true,
                FailureType = OrderStatusUpdateFailureType.None
            };
        }

        protected override void StartImpl()
        {
            OnConnected(); //TODO: we may no longer need to "start" the exchange or anything from it. Its just used as a service exposing bitfinex API 
        }

        protected override void StopImpl()
        {

        }



        private static OrderExecutionStatus ConvertExecutionStatus(Order order)
        {
            if (order.IsCancelled)
            {
                return OrderExecutionStatus.Cancelled;
            }
            if (order.IsLive)
            {
                return OrderExecutionStatus.New;
            }
            return OrderExecutionStatus.Fill;
        }

    }
}
