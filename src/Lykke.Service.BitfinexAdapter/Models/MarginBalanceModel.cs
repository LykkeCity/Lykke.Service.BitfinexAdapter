using Newtonsoft.Json;

namespace Lykke.Service.BitfinexAdapter.Models
{
    public sealed class MarginBalanceModel
    {
        [JsonProperty("accountCurrency")]
        public string AccountCurrency { get; set; }

        [JsonProperty("unrealisedPnL")]
        public decimal UnrealisedPnL { get; set; }

        [JsonProperty("marginBalance")]
        public decimal MarginBalance { get; set; }

        [JsonProperty("tradableBalance")]
        public decimal TradableBalance { get; set; }

        [JsonProperty("totalBalance")]
        public decimal TotalBalance { get; set; }

        [JsonProperty("marginUsed")]
        public decimal MarginUsed { get; set; }
    }
}
