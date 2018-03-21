namespace Lykke.Service.BitfinexAdapter.Core.Domain.Trading
{
    public sealed class TradingBalance
    {
        public string AccountCurrency { get; set; }

        public decimal Totalbalance { get; set; }

        public decimal UnrealisedPnL { get; set; }

        public decimal MaringAvailable { get; set; }

        public decimal MarginUsed { get; set; }
    }
}
