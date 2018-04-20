using Newtonsoft.Json;

namespace Lykke.Service.BitfinexAdapter.Core.Domain.WebSocketClient
{
    public class EventMessageResponse : EventResponse
    {

        [JsonProperty("code")]
        public Code Code { get; set; }

        [JsonProperty("msg")]
        public string Message { get; set; }
    }
}
