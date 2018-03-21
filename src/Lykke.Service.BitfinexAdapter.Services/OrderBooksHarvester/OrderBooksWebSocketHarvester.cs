using Common.Log;
using Lykke.Service.BitfinexAdapter.Core.Domain.OrderBooks;
using Lykke.Service.BitfinexAdapter.Core.Domain.Settings;
using Lykke.Service.BitfinexAdapter.Core.Handlers;
using Lykke.Service.BitfinexAdapter.Core.WebSocketClient;

namespace Lykke.Service.BitfinexAdapter.Services.OrderBooksHarvester
{
    public abstract class OrderBooksWebSocketHarvester<TRequest, TResponse> : OrderBooksHarvesterBase
    {
        protected IMessenger<TRequest, TResponse> Messenger;

        protected OrderBooksWebSocketHarvester(string exchangeName, BitfinexAdapterSettings adapterSettings, IMessenger<TRequest, TResponse> messanger, ILog log,
            IHandler<OrderBook> orderBookHandler)
            : base(exchangeName, adapterSettings, log, orderBookHandler)
        {
            Messenger = messanger;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose managed resources
                if (Messenger != null)
                {
                    Messenger.Dispose();
                    Messenger = null;
                }
            }
            base.Dispose(disposing);
        }
    }
}
