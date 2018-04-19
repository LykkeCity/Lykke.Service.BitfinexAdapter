using Lykke.Service.BitfinexAdapter.Core.Domain.Exchange;
using Lykke.Service.BitfinexAdapter.Core.Domain.Trading;
using Lykke.Service.BitfinexAdapter.Core.Domain.Trading.Enums;
using Lykke.Service.BitfinexAdapter.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.BitfinexAdapter.Core.Services
{
    public interface IExchange
    {
        string Name { get; }

        ExchangeState State { get; }

        IReadOnlyList<Instrument> Instruments { get; }

        Task<IReadOnlyCollection<MarginBalanceDomain>> GetMarginBalances(TimeSpan timeout);

        Task<IReadOnlyCollection<WalletBalance>> GetWalletBalances(TimeSpan timeout);

        Task<ExecutionReport> AddOrderAndWaitExecution(TradingSignal signal, TimeSpan timeout, long orderIdToReplace = 0);

        Task<long> CancelOrder(long orderId, TimeSpan timeout);

        Task<ExecutionReport> GetOrder(long id, TimeSpan timeout, OrderType orderType = OrderType.Unknown);

        Task<IEnumerable<ExecutionReport>> GetOpenOrders(TimeSpan timeout);

        Task<IEnumerable<ExecutionReport>> GetOrdersHistory(TimeSpan timeout);

        Task<IEnumerable<ExecutionReport>> GetLimitOrders(List<string> instrumentsFilter, List<long> orderIdFilter, bool isMarginRequest, TimeSpan timeout);

        Task<IReadOnlyCollection<TradingPosition>> GetPositionsAsync(TimeSpan timeout);

        Task<IReadOnlyList<string>> GetAllExchangeInstruments(TimeSpan timeout);





    }
}
