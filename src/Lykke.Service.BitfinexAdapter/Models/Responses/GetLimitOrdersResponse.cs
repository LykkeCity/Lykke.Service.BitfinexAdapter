using Newtonsoft.Json;
using System.Collections.Generic;

namespace Lykke.Service.BitfinexAdapter.Models.Responses
{
    public class GetLimitOrdersResponse
    {
        [JsonProperty("Orders")]
        public List<OrderModel> Orders { get; set; }
    }
}
