namespace Lykke.Service.BitfinexAdapter.Core.Domain.WebSocketClient
{
    public sealed class SublscribeTickeRequest : SubscribeChannelRequest
    {
        public static SublscribeTickeRequest BuildRequest(string pair)
        {
            return new SublscribeTickeRequest
            {
                Event = "subscribe",
                Pair = pair,
                Channel = WsChannel.ticker
            };
        }
    }
}
