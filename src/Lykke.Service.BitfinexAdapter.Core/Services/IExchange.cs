using Lykke.Service.BitfinexAdapter.Core.Domain.Exchange;
using Lykke.Service.BitfinexAdapter.Core.Domain.Trading;
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

        Task<IEnumerable<AccountBalance>> GetAccountBalance(TimeSpan timeout);

        Task<IReadOnlyCollection<TradingBalance>> GetTradeBalances(TimeSpan timeout);

        Task<ExecutionReport> AddOrderAndWaitExecution(TradingSignal signal, TimeSpan timeout);

        Task<ExecutionReport> CancelOrderAndWaitExecution(TradingSignal signal, TimeSpan timeout);

        Task<ExecutionReport> GetOrder(string id, Instrument instrument, TimeSpan timeout);

        Task<IEnumerable<ExecutionReport>> GetOpenOrders(TimeSpan timeout);

        Task<IReadOnlyCollection<TradingPosition>> GetPositionsAsync(TimeSpan timeout);

        StreamingSupport StreamingSupport { get; }

        Task<IReadOnlyList<string>> GetAllExchangeInstruments();


    }
}
