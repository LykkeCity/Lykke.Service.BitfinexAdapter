using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.BitfinexAdapter.Core.Domain.Settings
{
    public class CurrencySymbol
    {
        public string LykkeSymbol { get; set; }

        public string ExchangeSymbol { get; set; }

        [Optional]
        public bool OrderBookVolumeInQuoteCcy { get; set; }
    }
}
