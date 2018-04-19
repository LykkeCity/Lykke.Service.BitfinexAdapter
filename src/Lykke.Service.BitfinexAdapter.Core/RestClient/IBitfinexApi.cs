using Lykke.Service.BitfinexAdapter.Core.Domain.RestClient;
using Lykke.Service.BitfinexAdapter.Core.Domain.Trading;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Lykke.Service.BitfinexAdapter.Core.RestClient
{
    public interface IBitfinexApi : IDisposable
    {
        Task<Order> AddOrderAsync(NewOrderRequest orderRequest, CancellationToken cancellationToken = default);
        Task<Order> ReplaceOrderAsync(NewOrderRequest orderRequest, CancellationToken cancellationToken = default);
        Task<Order> CancelOrderAsync(long orderId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Order>> GetActiveOrdersAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Order>> GetInactiveOrdersAsync(CancellationToken cancellationToken = default);
        Task<Order> GetOrderStatusAsync(long orderId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<WalletBalance>> GetWalletBalancesAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<MarginInfo>> GetMarginInformationAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Position>> GetActivePositionsAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<string>> GetAllSymbolsAsync(CancellationToken cancellationToken = default);
    }
}
