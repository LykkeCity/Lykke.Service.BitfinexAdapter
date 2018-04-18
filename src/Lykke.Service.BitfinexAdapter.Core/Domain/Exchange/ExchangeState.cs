namespace Lykke.Service.BitfinexAdapter.Core.Domain.Exchange
{
    public enum ExchangeState
    {
        Initializing,
        Connecting,
        ReconnectingAfterError, //
        Connected,
        ReceivingPrices,
        ExecuteOrders,
        ErrorState,
        Stopped,
        Stopping
    }
}
