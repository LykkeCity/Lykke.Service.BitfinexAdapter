using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.BitfinexAdapter.Models.LimitOrders
{
    public class CancelLimitOrderRequest
    {
        [JsonProperty("orderId")]
        [Required]
        public long OrderId { get; set; }
    }
}
