using Autofac;
using Common.Log;
using Lykke.Service.BitfinexAdapter.Core.Domain.Settings;
using Lykke.Service.BitfinexAdapter.Core.Services;
using Lykke.Service.BitfinexAdapter.Services.ExecutionHarvester;
using Lykke.Service.BitfinexAdapter.Services.OrderBooksHarvester;
using System;
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

            if (_settings.RabbitMq.Trades.Enabled) //subscribe to order execution only if trades publishing is enabled
            {
                foreach (var clientApiKeySecret in _settings.Credentials)
                {
                    if (!String.IsNullOrWhiteSpace(clientApiKeySecret.Value.ApiKey) && !String.IsNullOrWhiteSpace(clientApiKeySecret.Value.ApiSecret) && _container.IsRegisteredWithName<BitfinexExecutionHarvester>(clientApiKeySecret.Value.ApiKey))
                    {
                        var harvester = _container.ResolveNamed<BitfinexExecutionHarvester>(clientApiKeySecret.Value.ApiKey);
                        harvester.Start();
                        await _log.WriteInfoAsync(nameof(StartupManager), nameof(StartAsync), $"{nameof(BitfinexExecutionHarvester)} started for client api key {clientApiKeySecret.Value.ApiKey}");
                    }
                }
            }
            
            await Task.CompletedTask;
        }
    }
}
