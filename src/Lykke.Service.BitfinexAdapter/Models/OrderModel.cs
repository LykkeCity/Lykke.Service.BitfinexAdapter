using Newtonsoft.Json;
using System;

namespace Lykke.Service.BitfinexAdapter.Models
{
    public class OrderModel
    {
        [JsonProperty("orderId")]
        public long Id { get; set; }

        [JsonProperty("instrument")]
        public string Symbol { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("originalAmount")]
        public decimal OriginalVolume { get; set; }

        [JsonProperty("tradeType")]
        public string TradeType { get; set; }

        [JsonProperty("createdTime")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("avgExecutionPrice")]
        public decimal AvgExecutionPrice { get; set; }

        [JsonProperty("status")]
        public string ExecutionStatus { get; set; }

        [JsonProperty("executedAmount")]
        public decimal ExecutedVolume { get; set; }

        [JsonProperty("remaining_amount")]
        public decimal RemainingAmount { get; set; }
    }
}
