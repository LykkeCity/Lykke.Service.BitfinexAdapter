using System.Threading.Tasks;

namespace Lykke.Service.BitfinexAdapter.Core.Services
{
    public interface IStartupManager
    {
        Task StartAsync();
    }
}