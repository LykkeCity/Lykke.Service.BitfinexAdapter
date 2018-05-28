using Newtonsoft.Json;

namespace Lykke.Service.BitfinexAdapter.Core.Domain.WebSocketClient
{
    public sealed class SubscribeOrderBooksRequest : SubscribeChannelRequest
    {
        public enum OrderBookLength
        {
            Default = 0,
            TwentyFive = 25,
            OneHundred = 100
        }

        [JsonProperty("freq")]
        public string Freq { get; set; }

        [JsonProperty("prec")]
        public string Prec { get; set; }

        [JsonProperty("len")]
        public int Length { get; set; }

        public static SubscribeOrderBooksRequest BuildRequest(
            string pair,
            string freq,
            string prec,
            OrderBookLength length)
        {
            return new SubscribeOrderBooksRequest
            {
                Event = "subscribe",
                Pair = pair,
                Channel = WsChannel.book,
                Freq = freq,
                Prec = prec,
                Length = (int)length
            };
        }

    }
}
