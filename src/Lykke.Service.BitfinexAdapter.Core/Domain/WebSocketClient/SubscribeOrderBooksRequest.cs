using Newtonsoft.Json;

namespace Lykke.Service.BitfinexAdapter.Core.Domain.WebSocketClient
{
    public sealed class SubscribeOrderBooksRequest : SubscribeChannelRequest
    {
        [JsonProperty("freq")]
        public string Freq { get; set; }

        [JsonProperty("prec")]
        public string Prec { get; set; }

        public static SubscribeOrderBooksRequest BuildRequest(string pair, string freq, string prec)
        {
            return new SubscribeOrderBooksRequest
            {
                Event = "subscribe",
                Pair = pair,
                Channel = WsChannel.book,
                Freq = freq,
                Prec = prec
            };
        }

    }
}
