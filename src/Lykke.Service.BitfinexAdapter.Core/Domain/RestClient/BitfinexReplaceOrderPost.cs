using Newtonsoft.Json;

namespace Lykke.Service.BitfinexAdapter.Core.Domain.RestClient
{
    public class BitfinexReplaceOrderPost : BitfinexNewOrderPost
    {
        [JsonProperty("order_id")]
        public long OrderIdToReplace { get; set; }
    }
}
