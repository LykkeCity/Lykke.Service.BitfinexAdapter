using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lykke.Service.BitfinexAdapter.Core.RestClient
{
    public interface IBitfinexApi : IDisposable
    {
        Task<object> AddOrderAsync(string symbol, decimal amount, decimal price, string side, string type, CancellationToken cancellationToken = default);
        Task<object> ReplaceOrderAsync(long orderIdToReplace, string symbol, decimal amount, decimal price, string side, string type, CancellationToken cancellationToken = default);
        Task<object> CancelOrderAsync(long orderId, CancellationToken cancellationToken = default);
        Task<object> GetActiveOrdersAsync(CancellationToken cancellationToken = default);
        Task<object> GetInactiveOrdersAsync(CancellationToken cancellationToken = default);
        Task<object> GetOrderStatusAsync(long orderId, CancellationToken cancellationToken = default);
        Task<object> GetWalletBalancesAsync(CancellationToken cancellationToken = default);
        Task<object> GetMarginInformationAsync(CancellationToken cancellationToken = default);
        Task<object> GetActivePositionsAsync(CancellationToken cancellationToken = default);
        Task<object> GetAllSymbolsAsync(CancellationToken cancellationToken = default);
    }
}
