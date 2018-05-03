using Lykke.Service.BitfinexAdapter.Core.Settings.ServiceSettings;
using Lykke.SettingsReader.Attributes;
using System.Collections.Generic;

namespace Lykke.Service.BitfinexAdapter.Core.Domain.Settings
{
    public sealed class BitfinexAdapterSettings 
    {
        public DbSettings Db { get; set; }
        public RabbitMqConfiguration RabbitMq { get; set; }


        public BitfinexAdapterSettings()
        {
            UseSupportedCurrencySymbolsAsFilter = true;
        }

        /// <summary>
        /// Use SupportedCurrencySymbols as filter of instrument for stream to rabbitmq
        /// true or null - provide only this instrument with mapping name
        /// false - provide all instrument and mapping name use this array
        /// </summary>
        [Optional]
        public bool UseSupportedCurrencySymbolsAsFilter { get; set; }

        public Dictionary<string, ApiKeyCredentials> Credentials { get; set; }

        public string EndpointUrl { get; set; }

        public string WebSocketEndpointUrl { get; set; }

        public int MaxEventPerSecondByInstrument { get; set; }

        public IReadOnlyCollection<CurrencySymbol> SupportedCurrencySymbols { get; set; }
    }
}
