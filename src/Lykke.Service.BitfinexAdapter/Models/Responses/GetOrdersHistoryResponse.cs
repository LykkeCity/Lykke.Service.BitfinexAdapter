using Newtonsoft.Json;
using System.Collections.Generic;

namespace Lykke.Service.BitfinexAdapter.Models.Responses
{
    public class GetOrdersHistoryResponse
    {
        [JsonProperty("Orders")]
        public List<OrderModel> Orders { get; set; }
    }
}
