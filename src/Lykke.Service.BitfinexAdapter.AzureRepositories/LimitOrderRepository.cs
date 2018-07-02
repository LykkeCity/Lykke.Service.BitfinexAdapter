using System;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.BitfinexAdapter.Core.Domain.RestClient;

namespace Lykke.Service.BitfinexAdapter.AzureRepositories
{
    public sealed class LimitOrderRepository : ILimitOrderRepository
    {
        private readonly INoSQLTableStorage<LimitOrderEntity> _storage;

        public LimitOrderRepository(
            INoSQLTableStorage<LimitOrderEntity> storage)
        {
            _storage = storage;
        }

        public Task CreateNewEntity(string xApiKey, Order order)
        {
            var entity = LimitOrderEntity.ByOrder.Create(order, xApiKey, DateTime.UtcNow);
            entity.Created = DateTime.UtcNow;
            return _storage.InsertAsync(entity);
        }

        public Task UpdateEntity(string xApiKey, Order order)
        {
            var entity = LimitOrderEntity.ByOrder.Create(order, xApiKey);
            entity.Timestamp = DateTimeOffset.UtcNow;
            return _storage.InsertOrMergeAsync(entity);
        }
    }
}
