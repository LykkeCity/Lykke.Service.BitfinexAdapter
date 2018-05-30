using System;
using System.Globalization;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using Lykke.Common.ExchangeAdapter.SpotController.Records;
using Lykke.Service.BitfinexAdapter.Core.Domain.RestClient;

namespace Lykke.Service.BitfinexAdapter.AzureRepositories
{
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateAlways)]
    public sealed class LimitOrderEntity : AzureTableEntity, IOrder
    {
        public string XApiKey { get; set; }
        public long Id { get; set; }
        public string Symbol { get; set; }
        public string Exchange { get; set; }
        public decimal Price { get; set; }
        public decimal AvgExecutionPrice { get; set; }
        public string TradeType { get; set; }
        public string OrderType { get; set; }
        public bool IsLive { get; set; }
        public bool IsCancelled { get; set; }
        public bool WasForced { get; set; }
        public decimal OriginalAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal ExecutedAmount { get; set; }
        public DateTime? Created { get; set; }
        public OrderStatus OrderStatus { get; set; }

        public static class ByOrder
        {
            public static string GeneratePartitionKey(long orderId)
            {
                var str = string.Format(CultureInfo.InvariantCulture, "{0:D4}", orderId);
                return str.Substring(str.Length - 4);
            }

            public static string GenerateRowKey(long orderId)
            {
                return orderId.ToString(CultureInfo.InvariantCulture);
            }

            public static LimitOrderEntity Create(IOrder order, string xApiKey, DateTime? created = null)
            {
                return new LimitOrderEntity
                {
                    XApiKey = xApiKey,
                    Id = order.Id,
                    Symbol = order.Symbol,
                    Exchange = order.Exchange,
                    Price = order.Price,
                    AvgExecutionPrice = order.AvgExecutionPrice,
                    TradeType = order.TradeType,
                    OrderType = order.OrderType,
                    IsLive = order.IsLive,
                    IsCancelled = order.IsCancelled,
                    WasForced = order.WasForced,
                    OriginalAmount = order.OriginalAmount,
                    RemainingAmount = order.RemainingAmount,
                    ExecutedAmount = order.ExecutedAmount,
                    OrderStatus = order.ConvertOrderStatus(),
                    Created = created,
                    PartitionKey = GeneratePartitionKey(order.Id),
                    RowKey = GenerateRowKey(order.Id)
                };
            }
        }
    }
}