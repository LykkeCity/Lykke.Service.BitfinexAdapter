using Castle.Core.Internal;
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
using System.Net;
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
            var credenitals = new BitfinexServiceClientCredentials(apiKey, secret); 
            _exchangeApi = new BitfinexApi(credenitals, log)
            {
                BaseUri = new Uri(configuration.EndpointUrl)
            };
        }
        
        public override async Task<ExecutionReport> AddOrderAndWaitExecution(TradingSignal signal, TimeSpan timeout, long orderIdToReplace = 0)
        {
            var symbol = _modelConverter.LykkeSymbolToExchangeSymbol(signal.Instrument.Name);
            var volume = signal.Volume;
            var orderType = signal.IsMarginOrder ? _modelConverter.ConvertToMarginOrderType(signal.OrderType) : _modelConverter.ConvertToSpotOrderType(signal.OrderType);
            var side = _modelConverter.ConvertTradeType(signal.TradeSide);
            var price = signal.Price == 0 ? 1 : signal.Price ?? 1;
            var cts = new CancellationTokenSource(timeout);

            var response = orderIdToReplace!=0 ? await _exchangeApi.ReplaceOrderAsync(orderIdToReplace, symbol, volume, price, side, orderType, cts.Token):
                                                 await _exchangeApi.AddOrderAsync(symbol, volume, price, side, orderType, cts.Token);
            if (response is Error error)
            {
                await LykkeLog.WriteInfoAsync(nameof(BitfinexExchange), nameof(AddOrderAndWaitExecution), $"Request for order create returned error from exchange: {error.Message}. Order details: {signal}");
                throw new ApiException(error.Message, error.HttpApiStatusCode); 
            }

            var trade = OrderToTrade((Order)response);
            return trade;
        }

        public override async Task<ExecutionReport> CancelOrderAndWaitExecution(TradingSignal signal, TimeSpan timeout)
        {
            if (!long.TryParse(signal.OrderId, out var id))
            {
                throw new ApiException("Bitfinex order id can be only integer", HttpStatusCode.BadRequest);
            }
            var response = await CancelOrderById(id, timeout);
            var trade = OrderToTrade(response);
            return trade;
        }

        public override async Task<long> CancelOrder(long orderId, TimeSpan timeout)
        {
            return (await CancelOrderById(orderId, timeout))?.Id ?? 0;
        }


        private async Task<Order> CancelOrderById(long orderId, TimeSpan timeout)
        {
            var response = await _exchangeApi.CancelOrderAsync(orderId, new CancellationTokenSource(timeout).Token);
            if (response is Error error)
            {
                await LykkeLog.WriteInfoAsync(nameof(BitfinexExchange), nameof(CancelOrder), $"Request for cancel orderId {orderId} returned error from exchange: {error.Message}");
                throw new ApiException(error.Message, error.HttpApiStatusCode);
            }
            return (Order)response;
        }

        public override async Task<ExecutionReport> GetOrder(long id, TimeSpan timeout)
        {
            var cts = new CancellationTokenSource(timeout);
            var response = await _exchangeApi.GetOrderStatusAsync(id, cts.Token);
            if (response is Error error)
            {
                await LykkeLog.WriteInfoAsync(nameof(BitfinexExchange), nameof(GetOrder), $"Request for orderId {id} returned error from exchange: {error.Message}");
                throw new ApiException(error.Message, error.HttpApiStatusCode);
            }
            var trade = OrderToTrade((Order)response);
            return trade;
        }

        public override async Task<IEnumerable<ExecutionReport>> GetOpenOrders(TimeSpan timeout)
        {
            return await GetActiveOrders(timeout);
        }

        private async Task<IEnumerable<ExecutionReport>> GetActiveOrders(TimeSpan timeout)
        {
            var cts = new CancellationTokenSource(timeout);
            var response = await _exchangeApi.GetActiveOrdersAsync(cts.Token);
            if (response is Error error)
            {
                await LykkeLog.WriteInfoAsync(nameof(BitfinexExchange), nameof(GetOpenOrders), $"Request for all active orders returned error from exchange: {error.Message}");
                throw new ApiException(error.Message, error.HttpApiStatusCode);
            }
            var trades = ((IReadOnlyCollection<Order>)response).Select(OrderToTrade);
            return trades;
        }

        public override async Task<IEnumerable<ExecutionReport>> GetLimitOrders(List<string> instrumentsFilter, List<long> orderIdFilter, bool isMarginRequest, TimeSpan timeout)
        {
            var orders = await GetActiveOrders(timeout);

            orders = isMarginRequest ? orders.Where(o => o.TradeType == _modelConverter.ConvertToMarginOrderType(OrderType.Limit)) : 
                                       orders.Where(o => o.TradeType == _modelConverter.ConvertToSpotOrderType(OrderType.Limit));

            if (!orderIdFilter.IsNullOrEmpty())
            {
                orders = orders.Where(o => orderIdFilter.Contains(o.ExchangeOrderId));
            }

            if (!instrumentsFilter.IsNullOrEmpty())
            {
                orders = orders.Where(o => instrumentsFilter.Any(i=>i.Equals(o.Instrument.Name, StringComparison.InvariantCultureIgnoreCase)));
            }

            return orders;
        }

        public override async Task<IReadOnlyCollection<TradingPosition>> GetPositionsAsync(TimeSpan timeout)
        {
            var cts = new CancellationTokenSource(timeout);
            var response = await _exchangeApi.GetActivePositionsAsync(cts.Token);
            if (response is Error error)
            {
                throw new ApiException(error.Message, error.HttpApiStatusCode);
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
                throw new ApiException(error.Message, error.HttpApiStatusCode);
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
                throw new ApiException(error.Message, error.HttpApiStatusCode);
            }
            return (IReadOnlyList<WalletBalance>)balances;
        }


        private async Task<IReadOnlyList<MarginInfo>> GetMarginInfo(TimeSpan timeout)
        {
            var cts = new CancellationTokenSource(timeout);
            var response = await _exchangeApi.GetMarginInformationAsync(cts.Token);
            if (response is Error error)
            {
                throw new ApiException(error.Message, error.HttpApiStatusCode);
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
            var originalVolume = order.OriginalAmount;
            var tradeType = BitfinexModelConverter.ConvertTradeType(order.Side);
            var status = ConvertExecutionStatus(order);
            var instr = _modelConverter.ExchangeSymbolToLykkeInstrument(order.Symbol);

            return new ExecutionReport(instr, execTime, execPrice, originalVolume, tradeType, id, status, order.Type)
            {
                ExecType = ExecType.Trade,
                Success = true,
                FailureType = OrderStatusUpdateFailureType.None,
                RemainingVolume = order.RemainingAmount,
                
                
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
