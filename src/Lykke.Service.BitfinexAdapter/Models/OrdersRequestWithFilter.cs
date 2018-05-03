namespace Lykke.Service.BitfinexAdapter.Models
{
    public class OrdersRequestWithFilter
    {
        public long[] Ids { get; set; }
        public string[] Instruments { get; set; }
    }
}
