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

        [JsonProperty("tradeType")]
        public string Side { get; set; }

        [JsonProperty("orderType")]
        public string Type { get; set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("volume")]
        public decimal Volume { get; set; }

        [JsonProperty("executionStatus")]
        public string ExecutionStatus { get; set; }

        [JsonProperty("remaining_amount")]
        public decimal RemainingAmount { get; set; }
    }
}
