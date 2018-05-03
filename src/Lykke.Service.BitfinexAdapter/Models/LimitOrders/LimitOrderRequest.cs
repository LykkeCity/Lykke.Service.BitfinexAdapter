using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using Lykke.Service.BitfinexAdapter.Core.Domain.Trading.Enums;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.BitfinexAdapter.Models.LimitOrders
{
    public class LimitOrderRequest
    {
        /// <summary>
        /// name of instrument (asset pair)
        /// </summary>
        [JsonProperty("instrument")]
        [Required]
        public string Instrument { get; set; }

        /// <summary>
        /// price of order
        /// </summary>
        [JsonProperty("price")]
        [Required]
        [PositiveDecimalAttribute]
        public decimal Price { get; set; }

        /// <summary>
        /// volume of order
        /// </summary>
        [JsonProperty("amount")]
        [Required]
        [PositiveDecimalAttribute]
        public decimal Volume { get; set; }

        /// <summary>
        /// side of trade: Buy, Sell
        /// </summary>
        [JsonProperty("tradeType")]
        [JsonConverter(typeof(StringEnumConverter))]
        [StrictEnumChecker]
        [Required]
        public TradeType TradeType { get; set; }

    }
}
