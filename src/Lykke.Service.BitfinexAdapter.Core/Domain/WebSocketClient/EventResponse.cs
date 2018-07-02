using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Lykke.Service.BitfinexAdapter.Core.Domain.WebSocketClient
{
    public abstract class EventResponse
    {
        [JsonProperty("event")]
        public string Event { get; set; }

        public static EventResponse Parse(string json)
        {
            return Parse(JToken.Parse(json));
        }

        public static EventResponse Parse(JToken token)
        {
            if (token.Type == JTokenType.Array || token.All(t => t.Path != "event"))
            {
                return null;
            }

            var eventType = token["event"].Value<string>();
            EventResponse response = null;

            switch (eventType)
            {
                case PongResponse.Tag:
                    response = token.ToObject<PongResponse>();
                    break;
                case "auth":
                    response = token.ToObject<AuthMessageResponse>();
                    break;
                case "error":
                    response = token.ToObject<ErrorEventMessageResponse>();
                    break;
                case "info":
                    if (token.Any(t => t.Path == "code"))
                    {
                        response = token.ToObject<EventMessageResponse>();
                    }
                    else
                    {
                        response = token.ToObject<InfoResponse>();
                    }
                    break;
                case "subscribed":
                    response = token.ToObject<SubscribedResponse>();
                    break;
            }
            return response;
        }
    }
}
