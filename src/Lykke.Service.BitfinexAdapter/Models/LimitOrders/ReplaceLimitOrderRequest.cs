using Newtonsoft.Json;

namespace Lykke.Service.BitfinexAdapter.Models.LimitOrders
{
    public class ReplaceLimitOrderRequest : LimitOrderRequest
    {
        /// <summary>
        /// id of order for cancel(repalce)
        /// </summary>
        [JsonProperty("orderIdToCancel")]
        public long OrderIdToCancel { get; set; }
    }
}
