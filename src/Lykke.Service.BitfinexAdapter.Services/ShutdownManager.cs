using Autofac;
using Common;
using Common.Log;
using Lykke.Service.BitfinexAdapter.Core.Domain.Settings;
using Lykke.Service.BitfinexAdapter.Core.Services;
using Lykke.Service.BitfinexAdapter.Services.ExecutionHarvester;
using Lykke.Service.BitfinexAdapter.Services.OrderBooksHarvester;
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
        private readonly BitfinexOrderBooksHarvester _orderBooksHarvester;
        private BitfinexAdapterSettings _settings;
        private IComponentContext _container;

        public ShutdownManager(ILog log, BitfinexOrderBooksHarvester orderBooksHarvester, BitfinexAdapterSettings settings, IComponentContext container)
        {
            _log = log;
            _orderBooksHarvester = orderBooksHarvester;
            _settings = settings;
            _container = container;
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

            _orderBooksHarvester.Stop();
            await _log.WriteInfoAsync(nameof(ShutdownManager), nameof(StopAsync), $"{nameof(BitfinexOrderBooksHarvester)} stopped.");

            foreach (var clientApiKeyCredentials in _settings.Credentials)
            {
                var executionHarvester = _container.IsRegisteredWithName<BitfinexExecutionHarvester>(clientApiKeyCredentials.Key) ? _container.ResolveNamed<BitfinexExecutionHarvester>(clientApiKeyCredentials.Key) : null;
                if (executionHarvester != null)
                {
                    executionHarvester.Stop();
                    await _log.WriteInfoAsync(nameof(StartupManager), nameof(StopAsync), $"{nameof(BitfinexExecutionHarvester)} stopped for client api key {clientApiKeyCredentials.Key}");
                }
            }

            await Task.CompletedTask;
        }
    }
}
