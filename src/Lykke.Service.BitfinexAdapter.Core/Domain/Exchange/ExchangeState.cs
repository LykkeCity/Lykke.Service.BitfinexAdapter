namespace Lykke.Service.BitfinexAdapter.Core.Domain.Exchange
{
    public enum ExchangeState
    {
        Initializing,
        Connecting,
        Connected,
        ErrorState,
        Stopped,
        Stopping
    }
}
