using Common.Log;
using Lykke.Service.BitfinexAdapter.Core.Domain.Exchange;
using Lykke.Service.BitfinexAdapter.Core.Domain.Settings;
using Lykke.Service.BitfinexAdapter.Core.Domain.Trading;
using Lykke.Service.BitfinexAdapter.Core.Services;
using Lykke.Service.BitfinexAdapter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lykke.Service.BitfinexAdapter.Services.Exchange
{
    public abstract class ExchangeBase : IExchange
    {
        protected readonly ILog LykkeLog;

        public string Name { get; }

        internal BitfinexAdapterSettings Config { get; }

        public ExchangeState State { get; private set; }

        public IReadOnlyList<Instrument> Instruments { get; }

        protected ExchangeBase(string name, BitfinexAdapterSettings config, ILog log)
        {
            Name = name;
            Config = config;
            State = ExchangeState.Initializing;
            LykkeLog = log;

            Instruments = config.SupportedCurrencySymbols?.Select(x => new Instrument(Name, x.LykkeSymbol)).ToList() ?? new List<Instrument>();

            if (!Instruments.Any() && config.UseSupportedCurrencySymbolsAsFilter != false)
            {
                throw new ArgumentException($"There is no instruments in the settings for {Name} exchange");
            }
        }

        public void Start()
        {
            LykkeLog.WriteInfoAsync(nameof(ExchangeBase), nameof(Start), Name, $"Starting exchange {Name}, current state is {State}").Wait();

            if (State != ExchangeState.ErrorState && State != ExchangeState.Stopped && State != ExchangeState.Initializing)
                return;

            State = ExchangeState.Connecting;
            StartImpl();
        }

        protected abstract void StartImpl();
        public event Action Connected;
        protected void OnConnected()
        {
            State = ExchangeState.Connected;
            Connected?.Invoke();
        }

        public void Stop()
        {
            if (State == ExchangeState.Stopped)
                return;

            State = ExchangeState.Stopping;
            StopImpl();
        }

        protected abstract void StopImpl();
        public event Action Stopped;
        protected void OnStopped()
        {
            State = ExchangeState.Stopped;
            Stopped?.Invoke();
        }

        public abstract Task<ExecutionReport> AddOrderAndWaitExecution(TradingSignal signal,TimeSpan timeout);

        public abstract Task<ExecutionReport> CancelOrderAndWaitExecution(TradingSignal signal, TimeSpan timeout);

        public virtual Task<ExecutionReport> GetOrder(string id, Instrument instrument, TimeSpan timeout)
        {
            throw new NotSupportedException($"{Name} does not support receiving order information by {nameof(id)} and {nameof(instrument)}");
        }

        public virtual Task<IEnumerable<AccountBalance>> GetAccountBalance(TimeSpan timeout)
        {
            return Task.FromResult(Enumerable.Empty<AccountBalance>());
        }

        public virtual Task<IReadOnlyCollection<TradingBalance>> GetTradeBalances(TimeSpan timeout)
        {
            throw new NotSupportedException();
        }

        public virtual Task<IEnumerable<ExecutionReport>> GetOpenOrders(TimeSpan timeout)
        {
            throw new NotSupportedException();
        }

        public virtual Task<IReadOnlyCollection<TradingPosition>> GetPositionsAsync(TimeSpan timeout)
        {
            throw new NotSupportedException();
        }

        public abstract StreamingSupport StreamingSupport { get; }
    }
}
