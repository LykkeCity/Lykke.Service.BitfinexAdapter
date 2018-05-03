using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.BitfinexAdapter.Models.Responses
{
    public class CancelLimitOrderResponse
    {
        [JsonProperty("orderId")]
        [Required]
        public long OrderId { get; set; }
    }
}
