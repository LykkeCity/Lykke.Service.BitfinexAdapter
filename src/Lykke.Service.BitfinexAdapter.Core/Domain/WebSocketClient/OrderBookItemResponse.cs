namespace Lykke.Service.BitfinexAdapter.Core.Domain.WebSocketClient
{
    public class OrderBookItemResponse
    {
        public long Id { get; set; }

        public decimal Price { get; set; }

        public decimal Amount { get; set; }

        public string Pair { get; set; }

    }
}
