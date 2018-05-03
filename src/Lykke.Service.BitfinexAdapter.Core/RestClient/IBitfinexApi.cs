using Lykke.Service.BitfinexAdapter.Core.Domain.RestClient;
using Lykke.Service.BitfinexAdapter.Core.Domain.Trading;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace Lykke.Service.BitfinexAdapter.Core.RestClient
{
    public interface IBitfinexApi : IDisposable
    {
        Task<Order> AddOrderAsync(NewOrderRequest orderRequest, CancellationToken cancellationToken = default);
        Task<Order> ReplaceOrderAsync(NewOrderRequest orderRequest, CancellationToken cancellationToken = default);
        Task<Order> CancelOrderAsync(long orderId, CancellationToken cancellationToken = default);
        Task<ReadOnlyCollection<Order>> GetActiveOrdersAsync(CancellationToken cancellationToken = default);
        Task<ReadOnlyCollection<Order>> GetInactiveOrdersAsync(CancellationToken cancellationToken = default);
        Task<Order> GetOrderStatusAsync(long orderId, CancellationToken cancellationToken = default);
        Task<ReadOnlyCollection<WalletBalance>> GetWalletBalancesAsync(CancellationToken cancellationToken = default);
        Task<ReadOnlyCollection<MarginInfo>> GetMarginInformationAsync(CancellationToken cancellationToken = default);
        Task<ReadOnlyCollection<Position>> GetActivePositionsAsync(CancellationToken cancellationToken = default);
        Task<ReadOnlyCollection<string>> GetAllSymbolsAsync(CancellationToken cancellationToken = default);
    }
}
