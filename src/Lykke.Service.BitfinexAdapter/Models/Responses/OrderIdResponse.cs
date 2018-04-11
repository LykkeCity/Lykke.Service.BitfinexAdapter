using Newtonsoft.Json;

namespace Lykke.Service.BitfinexAdapter.Models.Responses
{
    public class OrderIdResponse
    {
        [JsonProperty("id")]
        public string OrderId { get; set; }
    }
}
