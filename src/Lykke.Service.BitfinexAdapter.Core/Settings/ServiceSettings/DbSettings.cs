using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.BitfinexAdapter.Core.Settings.ServiceSettings
{
    public class DbSettings
    {
        [AzureTableCheck]
        public string LogsConnString { get; set; }

        //public string EntitiesConnString { get; set; }
        //public string EntitiesTableName { get; set; }
    }
}
