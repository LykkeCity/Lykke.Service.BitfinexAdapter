using Newtonsoft.Json;

namespace Lykke.Service.BitfinexAdapter.Core.Domain.WebSocketClient
{
    public sealed class InfoResponse : EventResponse
    {
        [JsonProperty("version")]
        public string Version { get; set; }
    }
}
