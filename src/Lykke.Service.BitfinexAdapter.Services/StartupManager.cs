using Autofac;
using Common.Log;
using Lykke.Service.BitfinexAdapter.Core.Domain.Settings;
using Lykke.Service.BitfinexAdapter.Core.Services;
using Lykke.Service.BitfinexAdapter.Services.ExecutionHarvester;
using Lykke.Service.BitfinexAdapter.Services.OrderBooksHarvester;
using System.Threading.Tasks;

namespace Lykke.Service.BitfinexAdapter.Services
{
    // NOTE: Sometimes, startup process which is expressed explicitly is not just better, 
    // but the only way. If this is your case, use this class to manage startup.
    // For example, sometimes some state should be restored before any periodical handler will be started, 
    // or any incoming message will be processed and so on.
    // Do not forget to remove As<IStartable>() and AutoActivate() from DI registartions of services, 
    // which you want to startup explicitly.

    public class StartupManager : IStartupManager
    {
        private readonly ILog _log;
        //private readonly ExchangeBase _exchange;
        private readonly BitfinexOrderBooksHarvester _orderBooksHarvester;
        private BitfinexAdapterSettings _settings;
        private IComponentContext _container;

        public StartupManager(ILog log, BitfinexOrderBooksHarvester orderBooksHarvester, BitfinexAdapterSettings settings, IComponentContext container)
        {
            _log = log;
            _orderBooksHarvester = orderBooksHarvester;
            _settings = settings;
            _container = container;
        }

        public async Task StartAsync()
        {
            _orderBooksHarvester.Start();
            await _log.WriteInfoAsync(nameof(StartupManager), nameof(StartAsync), $"{nameof(BitfinexOrderBooksHarvester)} started.");

            foreach (var clientApiKeyCredentials in _settings.Credentials)
            {
                var executionHarvester = _container.IsRegisteredWithName<BitfinexExecutionHarvester>(clientApiKeyCredentials.Key) ? _container.ResolveNamed<BitfinexExecutionHarvester>(clientApiKeyCredentials.Key) : null;
                if (executionHarvester != null)
                {
                    executionHarvester.Start();
                    await _log.WriteInfoAsync(nameof(StartupManager), nameof(StartAsync), $"{nameof(BitfinexExecutionHarvester)} started for client api key {clientApiKeyCredentials.Key}");
                }
            }

            await Task.CompletedTask;
        }
    }
}
