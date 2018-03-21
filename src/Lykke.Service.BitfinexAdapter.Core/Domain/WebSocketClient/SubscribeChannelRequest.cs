using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.BitfinexAdapter.Core.Domain.WebSocketClient
{
    public abstract class SubscribeChannelRequest : SubscribeRequest
    {
        [JsonProperty("pair")]
        public string Pair { get; set; }

        [JsonProperty("channel")]
        [JsonConverter(typeof(StringEnumConverter))]
        public WsChannel Channel { get; set; }

    }
}
