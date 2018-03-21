namespace Lykke.Service.BitfinexAdapter.Core.Domain.Trading
{
    /// <summary>
    /// A description of the exchange streaming capabilities
    /// </summary>
    public sealed class StreamingSupport
    {
        /// <summary>
        /// Can stream order books
        /// </summary>
        public bool OrderBooks { get; }

        /// <summary>
        /// Can stream orders updates
        /// </summary>
        public bool Orders { get; }


        public StreamingSupport(bool orderBooks, bool orders)
        {
            OrderBooks = orderBooks;
            Orders = orders;
        }
    }
}
