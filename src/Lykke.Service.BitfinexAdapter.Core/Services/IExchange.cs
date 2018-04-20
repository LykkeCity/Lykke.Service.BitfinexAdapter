using Lykke.Service.BitfinexAdapter.Core.Domain.Exchange;
using Lykke.Service.BitfinexAdapter.Core.Domain.Trading;
using Lykke.Service.BitfinexAdapter.Core.Domain.Trading.Enums;
using Lykke.Service.BitfinexAdapter.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Lykke.Service.BitfinexAdapter.Core.Services
{
    public interface IExchange
    {
        string Name { get; }

        ExchangeState State { get; }

        IReadOnlyList<Instrument> Instruments { get; }

        Task<ReadOnlyCollection<MarginBalanceDomain>> GetMarginBalances(TimeSpan timeout);

        Task<ReadOnlyCollection<WalletBalance>> GetWalletBalances(TimeSpan timeout);

        Task<ExecutionReport> AddOrderAndWaitExecution(TradingSignal signal, TimeSpan timeout, long orderIdToReplace = 0);

        Task<long> CancelOrder(long orderId, TimeSpan timeout);

        Task<ExecutionReport> GetOrder(long id, TimeSpan timeout, OrderType orderType = OrderType.Unknown);

        Task<ReadOnlyCollection<ExecutionReport>> GetOpenOrders(TimeSpan timeout);

        Task<ReadOnlyCollection<ExecutionReport>> GetOrdersHistory(TimeSpan timeout);

        Task<ReadOnlyCollection<ExecutionReport>> GetLimitOrders(List<string> instrumentsFilter, List<long> orderIdFilter, bool isMarginRequest, TimeSpan timeout);

        Task<ReadOnlyCollection<TradingPosition>> GetPositionsAsync(TimeSpan timeout);

        Task<ReadOnlyCollection<string>> GetAllExchangeInstruments(TimeSpan timeout);





    }
}
