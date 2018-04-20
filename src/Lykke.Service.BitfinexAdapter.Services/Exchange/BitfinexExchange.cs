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

        private async Task<TResult> ExecuteApiMethod<TRequest, TResult>(Func<TRequest, CancellationToken, Task<TResult>> method, TRequest request, CancellationToken token)
        {
            try
            {
                var response = await method(request, token);
                return response;
            }
            catch (ApiException ex)
            {
                await LykkeLog.WriteErrorAsync(nameof(BitfinexExchange), method.Method.Name, request.ToString(), ex);
                throw;
            }
            catch (Exception ex)
            {
                await LykkeLog.WriteErrorAsync(nameof(BitfinexExchange), method.Method.Name, request.ToString(), ex );
                throw new ApiException(ex.Message, HttpStatusCode.InternalServerError);
            }
        }

        private async Task<TResult> ExecuteApiMethod<TResult>(Func<CancellationToken, Task<TResult>> method, CancellationToken token)
        {
            try
            {
                var response = await method(token);
                return response;
            }
            catch (ApiException ex)
            {
                await LykkeLog.WriteErrorAsync(nameof(BitfinexExchange), method.Method.Name, ex);
                throw;
            }
            catch (Exception ex)
            {
                await LykkeLog.WriteErrorAsync(nameof(BitfinexExchange), method.Method.Name, ex);
                throw new ApiException(ex.Message, HttpStatusCode.InternalServerError);
            }
        }

        public override async Task<ExecutionReport> AddOrderAndWaitExecution(TradingSignal signal, TimeSpan timeout, long orderIdToReplace = 0)
        {
            var symbol = _modelConverter.LykkeSymbolToExchangeSymbol(signal.Instrument.Name);
            var volume = signal.Volume;
            var orderType = signal.IsMarginOrder ? _modelConverter.ConvertToMarginOrderType(signal.OrderType) : _modelConverter.ConvertToSpotOrderType(signal.OrderType);
            var tradeType = _modelConverter.ConvertTradeType(signal.TradeType);
            var price = signal.Price == 0 ? 1 : signal.Price ?? 1;

            using (var cts = new CancellationTokenSource(timeout))
            {
                var newOrderRequest = new NewOrderRequest
                {
                    OrderIdToReplace = orderIdToReplace,
                    Symbol = symbol,
                    Аmount = volume,
                    Price = price,
                    Side = tradeType,
                    Type = orderType
                };

                var newOrderResponse = orderIdToReplace > 0 ? await ExecuteApiMethod(_exchangeApi.ReplaceOrderAsync, newOrderRequest, cts.Token) : 
                                                               await ExecuteApiMethod(_exchangeApi.AddOrderAsync, newOrderRequest, cts.Token); 

                var trade = OrderToTrade(newOrderResponse);
                return trade;
            }
        }

        public override async Task<long> CancelOrder(long orderId, TimeSpan timeout)
        {
            using (var cts = new CancellationTokenSource(timeout))
            {
                var response = await ExecuteApiMethod(_exchangeApi.CancelOrderAsync, orderId, cts.Token);
                return response.Id;
            }
        }

        public override async Task<ExecutionReport> GetOrder(long id, TimeSpan timeout, OrderType orderType = OrderType.Unknown)
        {
            using (var cts = new CancellationTokenSource(timeout))
            {
                var order = await ExecuteApiMethod(_exchangeApi.GetOrderStatusAsync, id, cts.Token);

                var orderTypeParsed = _modelConverter.GetOrderTypeFromString(order.OrderType);
                if (orderTypeParsed != orderType && orderType != OrderType.Unknown)
                {
                    throw new ApiException("Requested order id and type not found.", HttpStatusCode.NotFound);
                }

                var trade = OrderToTrade(order);
                return trade;
            }
        }

        public override async Task<IEnumerable<ExecutionReport>> GetOpenOrders(TimeSpan timeout)
        {
            return await GetOrders(_exchangeApi.GetActiveOrdersAsync,timeout);
        }

        public override async Task<IEnumerable<ExecutionReport>> GetOrdersHistory(TimeSpan timeout)
        {
            return await GetOrders(_exchangeApi.GetInactiveOrdersAsync, timeout);
        }

        private async Task<IEnumerable<ExecutionReport>> GetOrders(Func<CancellationToken, Task<IReadOnlyList<Order>>> apiOrdersCall, TimeSpan timeout)
        {
            using (var cts = new CancellationTokenSource(timeout))
            {
                var response = await ExecuteApiMethod(apiOrdersCall, cts.Token);
                var trades = response.Select(OrderToTrade);
                return trades;
            }
        }

        public override async Task<IEnumerable<ExecutionReport>> GetLimitOrders(List<string> instrumentsFilter, List<long> orderIdFilter, bool isMarginRequest, TimeSpan timeout)
        {
            var orders = await GetOrders(_exchangeApi.GetActiveOrdersAsync, timeout); 

            orders = isMarginRequest ? orders.Where(o => o.OrderType.Equals(_modelConverter.ConvertToMarginOrderType(OrderType.Limit), StringComparison.InvariantCultureIgnoreCase)) : 
                                       orders.Where(o => o.OrderType.Equals(_modelConverter.ConvertToSpotOrderType(OrderType.Limit), StringComparison.InvariantCultureIgnoreCase));

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
            using (var cts = new CancellationTokenSource(timeout))
            {
                var response = await ExecuteApiMethod(_exchangeApi.GetActivePositionsAsync, cts.Token);

                var marginInfo = await GetMarginInfo(timeout);
                var positions = ExchangePositionsToPositionModel(response, marginInfo);
                return positions;
            }
        }

        public override async Task<IReadOnlyList<string>> GetAllExchangeInstruments(TimeSpan timeout)
        {
            using (var cts = new CancellationTokenSource(timeout))
            {
                var response = await ExecuteApiMethod(_exchangeApi.GetAllSymbolsAsync, cts.Token); 

                var instrumentsFromExchange = (response).Select(i => i.ToUpper()).ToList();
                return instrumentsFromExchange;
            }
        }

        private IReadOnlyCollection<TradingPosition> ExchangePositionsToPositionModel(IEnumerable<Position> response, IReadOnlyList<MarginInfo> marginInfo)
        {
            var marginByCurrency = marginInfo[0].MarginLimits.ToDictionary(ml => ml.OnPair, ml => ml, StringComparer.InvariantCultureIgnoreCase);
            var result = response.Select(r =>
                new TradingPosition
                {
                    Symbol = r.Symbol, //_modelConverter.ExchangeSymbolToLykkeInstrument(r.Symbol).Name,
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
            using (var cts = new CancellationTokenSource(timeout))
            {
                var balances = await ExecuteApiMethod(_exchangeApi.GetWalletBalancesAsync, cts.Token);
                return balances;
            }
        }

        private async Task<IReadOnlyList<MarginInfo>> GetMarginInfo(TimeSpan timeout)
        {
            using (var cts = new CancellationTokenSource(timeout))
            {
                var response = await ExecuteApiMethod(_exchangeApi.GetMarginInformationAsync, cts.Token); 
                return response;
            }
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
            var orderPrice = order.Price;
            var originalVolume = order.OriginalAmount;
            var tradeType = BitfinexModelConverter.ConvertTradeType(order.TradeType);
            var status = ConvertExecutionStatus(order);

            return new ExecutionReport(new Instrument(order.Symbol), execTime, orderPrice, originalVolume, order.ExecutedAmount, tradeType, id, status, order.OrderType, order.AvgExecutionPrice)
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
                return OrderExecutionStatus.Active;
            }
            return OrderExecutionStatus.Fill;
        }

    }
}
