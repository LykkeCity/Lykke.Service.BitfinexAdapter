using System.Runtime.Serialization;

namespace Lykke.Service.BitfinexAdapter.Core.Domain.Trading.Enums
{
    public enum TradeType
    {
        [EnumMember(Value = "Buy")] Buy,
        [EnumMember(Value = "Sell")] Sell
    }
}
