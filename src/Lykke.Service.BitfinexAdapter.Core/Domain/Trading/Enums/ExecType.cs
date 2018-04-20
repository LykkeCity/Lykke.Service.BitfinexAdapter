namespace Lykke.Service.BitfinexAdapter.Core.Domain.Trading.Enums
{
    public enum ExecType
    {
        Unknown,
        New,
        PartialFill,
        Fill,
        DoneForDay,
        Cancelled,
        Replace,
        PendingCancel,
        Stopped,
        Rejected,
        Suspended,
        PendingNew,
        Calculated,
        Expired,
        Restarted,
        PendingReplace,
        Trade,
        TradeCorrect,
        TradeCancel,
        OrderStatus
    }
}
