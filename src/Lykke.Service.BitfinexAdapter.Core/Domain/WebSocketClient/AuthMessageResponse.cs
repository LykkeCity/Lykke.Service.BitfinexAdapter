using Newtonsoft.Json;

namespace Lykke.Service.BitfinexAdapter.Core.Domain.WebSocketClient
{
    public sealed class AuthMessageResponse : EventMessageResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("userId")]
        public long UserId { get; set; }
    }
}
