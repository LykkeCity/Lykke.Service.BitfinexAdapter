namespace Lykke.Service.BitfinexAdapter.Core.Domain.Trading.Enums
{
    public enum OrderStatusUpdateFailureType
    {
        None,
        Unknown,
        ExchangeError,
        ConnectorError,
        InsufficientFunds
    }
}
