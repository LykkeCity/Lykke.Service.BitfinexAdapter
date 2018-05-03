using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common;
using Common.Log;
using Lykke.Service.BitfinexAdapter.Core.Domain.OrderBooks;
using Lykke.Service.BitfinexAdapter.Core.Domain.Trading;
using Lykke.Service.BitfinexAdapter.Core.Handlers;
using Lykke.Service.BitfinexAdapter.Core.Services;
using Lykke.Service.BitfinexAdapter.Core.Settings;
using Lykke.Service.BitfinexAdapter.Core.Settings.ServiceSettings;
using Lykke.Service.BitfinexAdapter.Core.Throttling;
using Lykke.Service.BitfinexAdapter.Core.Utils;
using Lykke.Service.BitfinexAdapter.Core.WebSocketClient;
using Lykke.Service.BitfinexAdapter.Services;
using Lykke.Service.BitfinexAdapter.Services.Exchange;
using Lykke.Service.BitfinexAdapter.Services.ExecutionHarvester;
using Lykke.Service.BitfinexAdapter.Services.OrderBooksHarvester;
using Lykke.SettingsReader;
using Microsoft.Extensions.DependencyInjection;
using System;

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
            // ex:
            //  builder.RegisterType<QuotesPublisher>()
            //      .As<IQuotesPublisher>()
            //      .WithParameter(TypedParameter.From(_settings.CurrentValue.QuotesPublication))

            builder.RegisterInstance(_log)
                .As<ILog>()
                .SingleInstance();

            builder.RegisterType<HealthService>()
                .As<IHealthService>()
                .SingleInstance();

            builder.RegisterType<StartupManager>()
                .As<IStartupManager>();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>();

            builder.RegisterGeneric(typeof(RabbitMqHandler<>));

            builder.RegisterType<BitfinexOrderBooksHarvester>()
                .AsSelf()
                .As<IStopable>()
                .SingleInstance();

            builder.RegisterType<BitfinexModelConverter>().SingleInstance();

            builder.RegisterInstance(_settings.CurrentValue.BitfinexAdapterService);

            builder.RegisterType<BitfinexExchange>().As<ExchangeBase>().SingleInstance();

            RegisterRabbitMqHandler<TickPrice>(builder, _settings.CurrentValue.BitfinexAdapterService.RabbitMq.TickPrices, "tickHandler");
            RegisterRabbitMqHandler<ExecutionReport>(builder, _settings.CurrentValue.BitfinexAdapterService.RabbitMq.Trades);
            RegisterRabbitMqHandler<OrderBook>(builder, _settings.CurrentValue.BitfinexAdapterService.RabbitMq.OrderBooks, "orderBookHandler");

            builder.RegisterType<TickPriceHandlerDecorator>()
                .WithParameter((info, context) => info.Name == "rabbitMqHandler",
                    (info, context) => context.ResolveNamed<IHandler<TickPrice>>("tickHandler"))
                .SingleInstance()
                .As<IHandler<TickPrice>>();

            builder.RegisterType<EventsPerSecondPerInstrumentThrottlingManager>()
                .WithParameter("maxEventPerSecondByInstrument", _settings.CurrentValue.BitfinexAdapterService.MaxEventPerSecondByInstrument)
                .As<IThrottling>().InstancePerDependency();


            RegisterExecutionHarvesterForEachClient(builder);
            builder.Populate(_services);
        }

        private void RegisterExecutionHarvesterForEachClient(ContainerBuilder builder)
        {
            foreach (var clientApiKeySecret in _settings.CurrentValue.BitfinexAdapterService.Credentials)
            {
                if (!String.IsNullOrWhiteSpace(clientApiKeySecret.Value.ApiKey) && !String.IsNullOrWhiteSpace(clientApiKeySecret.Value.ApiSecret))
                {
                    var socketSubscriber = new BitfinexWebSocketSubscriber(_settings.CurrentValue.BitfinexAdapterService, true, _log, clientApiKeySecret.Value.ApiKey, clientApiKeySecret.Value.ApiSecret);
                    builder.RegisterType<BitfinexExecutionHarvester>()
                        .AsSelf()
                        .As<IStopable>()
                        .WithParameter("socketSubscriber", socketSubscriber)
                        .Named<BitfinexExecutionHarvester>(clientApiKeySecret.Value.ApiKey).SingleInstance();
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
