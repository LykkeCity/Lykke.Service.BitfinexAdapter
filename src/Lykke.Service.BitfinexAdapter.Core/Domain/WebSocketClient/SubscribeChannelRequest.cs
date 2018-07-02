using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.BitfinexAdapter.Core.Domain.WebSocketClient
{
    public abstract class SubscribeChannelRequest : SubscribeRequest
    {
        private string _pair;

        [JsonProperty("pair")]
        public string Pair
        {
            get => _pair;
            set => _pair = value?.ToUpperInvariant();
        }

        [JsonProperty("channel")]
        [JsonConverter(typeof(StringEnumConverter))]
        public WsChannel Channel { get; set; }

    }
}
