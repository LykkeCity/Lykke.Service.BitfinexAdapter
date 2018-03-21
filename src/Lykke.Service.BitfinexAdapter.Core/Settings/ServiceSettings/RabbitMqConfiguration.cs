namespace Lykke.Service.BitfinexAdapter.Core.Settings.ServiceSettings
{
    public class RabbitMqConfiguration
    {
        //order must be the same as in json settings 
        public RabbitMqExchangeConfiguration TickPrices { get; set; }
        public RabbitMqExchangeConfiguration Trades { get; set; }
        public RabbitMqExchangeConfiguration OrderBooks { get; set; }
    }
}
