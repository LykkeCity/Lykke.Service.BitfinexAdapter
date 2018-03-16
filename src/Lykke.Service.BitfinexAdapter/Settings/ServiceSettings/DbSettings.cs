using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.BitfinexAdapter.Settings.ServiceSettings
{
    public class DbSettings
    {
        [AzureTableCheck]
        public string LogsConnString { get; set; }
    }
}
