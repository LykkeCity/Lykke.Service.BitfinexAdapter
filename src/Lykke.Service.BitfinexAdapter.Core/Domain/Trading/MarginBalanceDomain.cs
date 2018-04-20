namespace Lykke.Service.BitfinexAdapter.Core.Domain.Trading
{
    public sealed class MarginBalanceDomain
    {
        public string AccountCurrency { get; set; }

        public decimal Totalbalance { get; set; }

        public decimal UnrealisedPnL { get; set; }

        public decimal MarginBalance { get; set; }

        public decimal TradableBalance { get; set; }

        public decimal MarginUsed { get; set; }
    }
}
