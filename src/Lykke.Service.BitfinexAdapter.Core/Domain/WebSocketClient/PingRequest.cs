namespace Lykke.Service.BitfinexAdapter.Core.Domain.WebSocketClient
{
    public sealed class PingRequest : SubscribeRequest
    {
        public PingRequest()
        {
            Event = "ping";
        }
    }
}
