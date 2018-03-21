namespace Lykke.Service.BitfinexAdapter.Core.Domain.Trading.Enums
{
    public enum OrderExecutionStatus
    {
        Unknown,
        Fill,
        PartialFill,
        Cancelled,
        Rejected,
        New,
        Pending
    }
}
