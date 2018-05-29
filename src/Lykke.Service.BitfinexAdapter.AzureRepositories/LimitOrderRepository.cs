using System;
using System.Globalization;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using Common.Log;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using Lykke.Common.ExchangeAdapter.SpotController.Records;
using Lykke.Service.BitfinexAdapter.Core.Domain.RestClient;
using Lykke.Service.BitfinexAdapter.Core.Domain.Settings;
using Lykke.SettingsReader;

namespace Lykke.Service.BitfinexAdapter.AzureRepositories
{
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateAlways)]
    internal sealed class LimitOrderEntity : AzureTableEntity, IOrder
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

        public static LimitOrderEntity FromOrder(IOrder order, string xApiKey, DateTime? created = null)
        {
            var (p, r) = GetKeys(order.Id);

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
                PartitionKey = p,
                RowKey = r
            };
        }

        private static (string, string) GetKeys(long orderId)
        {
            var str = $"{orderId:D4}";
            return (str.Substring(str.Length - 4), orderId.ToString(CultureInfo.InvariantCulture));
        }
    }

    public sealed class LimitOrderRepository
    {
        private readonly INoSQLTableStorage<LimitOrderEntity> _storage;

        public LimitOrderRepository(
            IReloadingManager<BitfinexAdapterSettings> connectionString,
            ILog log)
        {
            _storage = AzureTableStorage<LimitOrderEntity>.Create(
                connectionString.ConnectionString(x => x.SnapshotConnectionString),
                "BitfinexLimitOrders",
                log);
        }

        public Task CreateNewEntity(string xApiKey, Order order)
        {
            var entity = LimitOrderEntity.FromOrder(order, xApiKey, DateTime.UtcNow);
            entity.Created = DateTime.UtcNow;
            return _storage.InsertAsync(entity);
        }

        public Task UpdateEntity(string xApiKey, Order order)
        {
            var entity = LimitOrderEntity.FromOrder(order, xApiKey);
            entity.Timestamp = DateTimeOffset.UtcNow;
            return _storage.InsertOrMergeAsync(entity);
        }
    }
}
