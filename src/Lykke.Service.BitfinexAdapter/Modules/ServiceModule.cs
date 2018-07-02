using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using Lykke.Service.BitfinexAdapter.Core.Domain.Trading;
using Lykke.Service.BitfinexAdapter.Core.Handlers;
using Lykke.Service.BitfinexAdapter.Core.Services;
using Lykke.Service.BitfinexAdapter.Core.Settings;
using Lykke.Service.BitfinexAdapter.Core.Settings.ServiceSettings;
using Lykke.Service.BitfinexAdapter.Core.Utils;
using Lykke.Service.BitfinexAdapter.Core.WebSocketClient;
using Lykke.Service.BitfinexAdapter.Services;
using Lykke.Service.BitfinexAdapter.Services.Exchange;
using Lykke.Service.BitfinexAdapter.Services.ExecutionHarvester;
using Lykke.SettingsReader;
using Microsoft.Extensions.DependencyInjection;
using System;
using Lykke.Service.BitfinexAdapter.Services.OrderBooks;
using Microsoft.Extensions.Hosting;

namespace Lykke.Service.BitfinexAdapter.Modules
{
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;
        private readonly ILog _log;
        // NOTE: you can remove it if you don't need to use IServiceCollection extensions to register service specific dependencies
        private readonly IServiceCollection _services;

        public ServiceModule(IReloadingManager<AppSettings> settings, ILog log)
        {
            _settings = settings;
            _log = log;

            _services = new ServiceCollection();
        }

        protected override void Load(ContainerBuilder builder)
        {
            // TODO: Do not register entire settings in container, pass necessary settings to services which requires them

            builder.RegisterInstance(_log)
                .As<ILog>()
                .SingleInstance();

            builder.RegisterType<HealthService>()
                .As<IHealthService>()
                .SingleInstance();

            builder.RegisterGeneric(typeof(RabbitMqHandler<>));

            builder.RegisterType<BitfinexModelConverter>().SingleInstance();

            builder.RegisterInstance(_settings.CurrentValue.BitfinexAdapterService);

            builder.RegisterType<BitfinexExchange>().As<ExchangeBase>().SingleInstance();

            RegisterRabbitMqHandler<ExecutionReport>(builder, _settings.CurrentValue.BitfinexAdapterService.RabbitMq.Trades);

            builder.RegisterType<OrderBooksPublishingService>()
                .As<IHostedService>()
                .SingleInstance();

            RegisterExecutionHarvesterForEachClient(builder);
            builder.Populate(_services);
        }

        private void RegisterExecutionHarvesterForEachClient(ContainerBuilder builder)
        {
            foreach (var clientApiKeySecret in _settings.CurrentValue.BitfinexAdapterService.Credentials)
            {
                if (!String.IsNullOrWhiteSpace(clientApiKeySecret.ApiKey) && !String.IsNullOrWhiteSpace(clientApiKeySecret.ApiSecret))
                {
                    var socketSubscriber = new BitfinexWebSocketSubscriber(_settings.CurrentValue.BitfinexAdapterService, true, _log, clientApiKeySecret.ApiKey, clientApiKeySecret.ApiSecret);
                    builder.RegisterType<BitfinexExecutionHarvester>()
                        .AsSelf()
                        .As<IHostedService>()
                        .WithParameter("socketSubscriber", socketSubscriber)
                        .Named<BitfinexExecutionHarvester>(clientApiKeySecret.ApiKey).SingleInstance();
                }
            }
        }

        private static void RegisterRabbitMqHandler<T>(ContainerBuilder container, RabbitMqExchangeConfiguration exchangeConfiguration, string regKey = "")
        {
            container.RegisterType<RabbitMqHandler<T>>()
                .WithParameter("connectionString", exchangeConfiguration.ConnectionString)
                .WithParameter("exchangeName", exchangeConfiguration.Exchange)
                .WithParameter("enabled", exchangeConfiguration.Enabled)
                .Named<IHandler<T>>(regKey)
                .As<IHandler<T>>();
        }
    }
}
