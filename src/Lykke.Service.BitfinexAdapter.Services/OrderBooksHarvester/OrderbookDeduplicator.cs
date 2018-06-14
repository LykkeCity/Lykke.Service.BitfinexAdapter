using System.Collections.Concurrent;
using Common.Log;
using Lykke.Common.ExchangeAdapter.Contracts;

namespace Lykke.Service.BitfinexAdapter.Services.OrderBooksHarvester
{
    internal class OrderbookDeduplicator
    {
        private readonly ILog _log;

        private const int Multiplier = 10;

        public OrderbookDeduplicator(ILog log)
        {
            _log = log;
        }

        private readonly ConcurrentDictionary<string, TickPrice> _lastPrices
            = new ConcurrentDictionary<string, TickPrice>();

        public OrderbookDeduplicator(ConcurrentDictionary<string, TickPrice> lastPrices)
        {
            _lastPrices = lastPrices;
        }

        public bool IsOkToPublish(string pair, OrderBook ob)
        {
            var tickPrice = TickPrice.FromOrderBook(ob);

            if (_lastPrices.TryAdd(pair, tickPrice))
            {
                return true;
            }

            var lastPrice = _lastPrices[pair];

            if (lastPrice.Ask > tickPrice.Ask * Multiplier)
            {
                _log.WriteWarning(nameof(OrderbookDeduplicator), "",
                    $"New ask {tickPrice.Ask} at least {Multiplier} time smaller than previous {lastPrice.Ask}");
                return false;
            }

            if (lastPrice.Bid > tickPrice.Bid * Multiplier)
            {
                _log.WriteWarning(nameof(OrderbookDeduplicator), "",
                    $"New ask {tickPrice.Bid} at least {Multiplier} time smaller than previous {lastPrice.Bid}");
                return false;
            }

            _lastPrices[pair] = tickPrice;

            return true;
        }
    }
}
