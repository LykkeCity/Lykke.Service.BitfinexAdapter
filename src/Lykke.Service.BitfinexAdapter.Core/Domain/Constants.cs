namespace Lykke.Service.BitfinexAdapter.Core.Domain
{
    public static class Constants
    {
        public const string BitfinexExchangeName = "bitfinex";
        public const string XApiKeyHeaderName = "X-API-KEY";
        public static string AuthenticationError = "X-API-KEY was not set or the associated ApiKey and ApiSecret are missing from config.";
    }
}
