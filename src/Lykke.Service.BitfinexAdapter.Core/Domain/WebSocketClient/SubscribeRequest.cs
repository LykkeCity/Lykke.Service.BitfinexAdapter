using Newtonsoft.Json;

namespace Lykke.Service.BitfinexAdapter.Core.Domain.WebSocketClient
{
    public abstract class SubscribeRequest
    {
        [JsonProperty("event")]
        public string Event { get; set; }
    }
}
