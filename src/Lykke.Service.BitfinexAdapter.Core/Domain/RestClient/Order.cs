using Newtonsoft.Json;
using System;

namespace Lykke.Service.BitfinexAdapter.Core.Domain.RestClient
{
    public sealed class Order : IOrder
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("exchange")]
        public string Exchange { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("avg_execution_price")]
        public decimal AvgExecutionPrice { get; set; }

        [JsonProperty("side")]
        public string TradeType { get; set; }

        [JsonProperty("type")]
        public string OrderType { get; set; }

        [JsonIgnore]
        DateTimeOffset IOrder.Timestamp
        {
            get => new DateTimeOffset(Timestamp);
            set => Timestamp = value.DateTime;
        }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("is_live")]
        public bool IsLive { get; set; }

        [JsonProperty("is_cancelled")]
        public bool IsCancelled { get; set; }

        [JsonProperty("was_forced")]
        public bool WasForced { get; set; }

        [JsonProperty("original_amount")]
        public decimal OriginalAmount { get; set; }

        [JsonProperty("remaining_amount")]
        public decimal RemainingAmount { get; set; }

        [JsonProperty("executed_amount")]
        public decimal ExecutedAmount { get; set; }

        public override string ToString()
        {
            var str = $"New Order (Id: {Id}) Symb:{Symbol} {TradeType} Sz:{OriginalAmount} - Px:{Price}. (Type:{OrderType}, IsLive:{IsLive}, Executed Amt:{ExecutedAmount} - OrderId: {Id})" + $"(IsCancelled: {IsCancelled}, WasForced: {WasForced}, RemainingAmount: {RemainingAmount}, ExecutedAmount: {ExecutedAmount})";
            return str;
        }
    }

}
