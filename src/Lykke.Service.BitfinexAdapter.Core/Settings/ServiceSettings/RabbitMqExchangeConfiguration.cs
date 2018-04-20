namespace Lykke.Service.BitfinexAdapter.Core.Settings.ServiceSettings
{
    public class RabbitMqExchangeConfiguration
    {
        public bool Enabled { get; set; }

        public string Exchange { get; set; }

        public string ConnectionString { get; set; }
    }
}
