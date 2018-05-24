using Lykke.Service.BitfinexAdapter.Core.Domain.Trading;
using Lykke.Service.BitfinexAdapter.Core.Domain.Trading.Enums;
using System;
using System.Globalization;
using Lykke.Common.ExchangeAdapter.SpotController.Records;

namespace Lykke.Service.BitfinexAdapter.Models
{
    public static class ModelConverters
    {
        public static WalletBalanceModel ToApiModel(this WalletBalance wb)
        {
            return new WalletBalanceModel
            {
                Asset = wb.Currency,
                //Available = wb.Available,
                Balance = wb.Amount,
                Reserved = wb.Amount - wb.Available,
                //Type = wb.Type
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

        public static OrderModel ToApiModel(this ExecutionReport o)
        {
            return new OrderModel
            {
                Id = o.ExchangeOrderId.ToString(CultureInfo.InvariantCulture),
                Symbol = o.Instrument.Name,
                Price = o.Price,
                OriginalVolume = o.OriginalVolume,
                TradeType = o.tradeType,
                Timestamp = o.Time,
                AvgExecutionPrice = o.AvgExecutionPrice,
                ExecutionStatus = o.ExecutionStatus,
                ExecutedVolume = o.ExecutedVolume,
                RemainingAmount = o.RemainingVolume,
                
                
            };
        }


        public static TradingSignal ToLimitOrder(this LimitOrderRequest request, bool isMarginOrder)
        {
            return new TradingSignal(
                new Instrument(request.Instrument),
                orderId: null,
                command: OrderCommand.Create,
                tradeType: request.TradeType,
                price: request.Price,
                volume: request.Volume,
                time: DateTime.UtcNow,
                isMarginOrder: isMarginOrder,
                orderType: OrderType.Limit
                );
        }

        public static TradingSignal ToMarketOrder(this MarketOrderRequest request, bool isMarginOrder)
        {
            return new TradingSignal(
                new Instrument(request.Instrument),
                orderId: null,
                command: OrderCommand.Create,
                tradeType: request.TradeType,
                price: 0,
                volume: request.Volume,
                time: DateTime.UtcNow,
                isMarginOrder: isMarginOrder,
                orderType: OrderType.Market
            );
        }
    }
}
