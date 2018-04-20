using Common;
using Common.Log;
using Lykke.Service.BitfinexAdapter.Core.Services;
using Lykke.Service.BitfinexAdapter.Services.Exchange;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.BitfinexAdapter.Services
{
    // NOTE: Sometimes, shutdown process should be expressed explicitly. 
    // If this is your case, use this class to manage shutdown.
    // For example, sometimes some state should be saved only after all incoming message processing and 
    // all periodical handler was stopped, and so on.

    public class ShutdownManager : IShutdownManager
    {
        private readonly ILog _log;
        private readonly List<IStopable> _items = new List<IStopable>();
        private readonly ExchangeBase _exchange;

        public ShutdownManager(ILog log, ExchangeBase exchange)
        {
            _log = log;
            _exchange = exchange;
        }

        public void Register(IStopable stopable)
        {
            _items.Add(stopable);
        }

        public async Task StopAsync()
        {
            // TODO: Implement your shutdown logic here. Good idea is to log every step
            foreach (var item in _items)
            {
                item.Stop();
            }

            _exchange.Stop();
            await _log.WriteInfoAsync(nameof(ShutdownManager), nameof(StopAsync), $"{_exchange.Name} stopped.");

            await Task.CompletedTask;
        }
    }
}
