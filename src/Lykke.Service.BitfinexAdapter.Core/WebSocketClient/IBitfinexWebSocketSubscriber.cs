using System;
using System.Threading.Tasks;

namespace Lykke.Service.BitfinexAdapter.Core.WebSocketClient
{
    public interface IBitfinexWebSocketSubscriber
    {
        Task Subscribe(Func<dynamic, Task> handlerFunc);
        void Start();
        void Stop();
    }
}
