using System;
using Lykke.Common.ExchangeAdapter.SpotController.Records;

namespace Lykke.Service.BitfinexAdapter.Core.Domain.RestClient
{
    public static class OrderExtensions
    {
        public static OrderStatus ConvertOrderStatus(this IOrder order)
        {
            if (order.IsCancelled)
            {
                return OrderStatus.Canceled;
            }

            if (order.IsLive)
            {
                return OrderStatus.Active;
            }

            return OrderStatus.Fill;
        }
    }

    public interface IOrder
    {
        long Id { get; set; }
        string Symbol { get; set; }
        string Exchange { get; set; }
        decimal Price { get; set; }
        decimal AvgExecutionPrice { get; set; }
        string TradeType { get; set; }
        string OrderType { get; set; }
        DateTimeOffset Timestamp { get; set; }
        bool IsLive { get; set; }
        bool IsCancelled { get; set; }
        bool WasForced { get; set; }
        decimal OriginalAmount { get; set; }
        decimal RemainingAmount { get; set; }
        decimal ExecutedAmount { get; set; }
    }
}
