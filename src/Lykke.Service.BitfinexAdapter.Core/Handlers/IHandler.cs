using System.Threading.Tasks;

namespace Lykke.Service.BitfinexAdapter.Core.Handlers
{
    public interface IHandler<in T>
    {
        Task Handle(T message);
    }
}
