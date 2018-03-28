using Lykke.Service.BitfinexAdapter.Core.Domain.Trading;

namespace Lykke.Service.BitfinexAdapter.Models
{
    public static class ModelConverters
    {
        public static WalletBalanceModel ToApiModel(this WalletBalance wb)
        {
            return new WalletBalanceModel
            {
                Asset = wb.Currency,
                Available = wb.Available,
                Balance = wb.Amount,
                Reserved = wb.Amount - wb.Available,
                Type = wb.Type
            };
        }

        public static MarginBalanceModel ToApiModel(this MarginBalanceDomain mb)
        {
            return new MarginBalanceModel
            {
                TradableBalance = mb.TradableBalance,
                AccountCurrency = mb.AccountCurrency,
                MarginBalance = mb.MarginBalance,
                MarginUsed = mb.MarginUsed,
                UnrealisedPnL = mb.UnrealisedPnL,
                TotalBalance = mb.Totalbalance
            };
        }
    }
}
