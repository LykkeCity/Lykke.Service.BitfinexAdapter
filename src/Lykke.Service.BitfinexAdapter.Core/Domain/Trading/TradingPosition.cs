namespace Lykke.Service.BitfinexAdapter.Models
{
    public class TradingPosition
    {
        public string Symbol { get; set; }

        public decimal PositionVolume { get; set; }

        public decimal MaintMarginUsed { get; set; }

        public decimal RealisedPnL { get; set; }

        public decimal UnrealisedPnL { get; set; }

        public decimal? PositionValue { get; set; }

        public decimal? AvailableMargin { get; set; }

        public decimal InitialMarginRequirement { get; set; }

        public decimal MaintenanceMarginRequirement { get; set; }
    }
}
