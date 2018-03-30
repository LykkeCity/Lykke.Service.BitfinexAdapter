using Lykke.Service.BitfinexAdapter.Core.Domain.Trading.Enums;
using Newtonsoft.Json;

namespace Lykke.Service.BitfinexAdapter.Models.LimitOrders
{
    public class LimitOrderRequest
    {
        /// <summary>
        /// name of instrument (asset pair)
        /// </summary>
        [JsonProperty("instrument")]
        public string Instrument { get; set; }

        /// <summary>
        /// price of order
        /// </summary>
        [JsonProperty("price")]
        public decimal Price { get; set; }

        /// <summary>
        /// volume of order
        /// </summary>
        [JsonProperty("volume")]
        public decimal Volume { get; set; }

        /// <summary>
        /// type of order: “Market”, “Limit”, “FillOrKill”
        /// </summary>
        [JsonProperty("orderType")]
        public OrderType OrderType { get; set; }

        /// <summary>
        /// side of trade: Buy, Sell
        /// </summary>
        [JsonProperty("tradeSide")]
        public TradeSide TradeSide { get; set; } //needs validation

    }
}
