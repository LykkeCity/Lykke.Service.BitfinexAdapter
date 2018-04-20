using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.BitfinexAdapter.Models.LimitOrders
{
    public class MarketOrderRequest
    {
        /// <summary>
        /// name of instrument (asset pair)
        /// </summary>
        [JsonProperty("instrument")]
        [Required]
        public string Instrument { get; set; }

        /// <summary>
        /// volume of order
        /// </summary>
        [JsonProperty("amount")]
        [Required]
        public decimal Volume { get; set; }

        /// <summary>
        /// side of trade: Buy, Sell
        /// </summary>
        [JsonProperty("tradeType")]
        [Required]
        public string TradeType { get; set; } //needs validation


    }
}
