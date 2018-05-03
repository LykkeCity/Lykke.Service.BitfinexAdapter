using Common;
using Common.Log;
using Lykke.Service.BitfinexAdapter.Core.Services;
using System.Collections.Generic;
using System.Linq;
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

        public ShutdownManager(ILog log, IEnumerable<IStopable> stopables) 
        {
            _log = log;
            _items = stopables.ToList();
        }

        public void Register(IStopable stopable)
        {
            _items.Add(stopable);
        }

        public async Task StopAsync()
        {
            foreach (var item in _items)
            {
                item.Stop();
                await _log.WriteInfoAsync(nameof(ShutdownManager), nameof(StopAsync), $"{item.GetType().Name} stopped.");
            }

            await Task.CompletedTask;
        }
    }
}
