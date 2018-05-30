using System.Threading.Tasks;
using Lykke.Service.BitfinexAdapter.Core.Domain.RestClient;

namespace Lykke.Service.BitfinexAdapter.AzureRepositories
{
    public interface ILimitOrderRepository
    {
        Task CreateNewEntity(string xApiKey, Order order);
        Task UpdateEntity(string xApiKey, Order order);
    }
}