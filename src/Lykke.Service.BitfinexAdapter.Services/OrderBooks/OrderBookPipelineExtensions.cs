using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.ExchangeAdapter;
using Lykke.Common.ExchangeAdapter.Contracts;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.BitfinexAdapter.Core.Utils;

namespace Lykke.Service.BitfinexAdapter.Services.OrderBooks
{
    public static class OrderBookPipelineExtensions
    {
        private static IObservable<OrderBook> DetectAndFilterAnomaliesAssumingSingleInstrument(
            this IObservable<OrderBook> source,
            ILog log)
        {
            decimal? MidPrice(decimal ask, decimal bid)
            {
                if (ask == 0 || bid == 0) return null;
                return (ask + bid) / 2;
            }

            string DetectAnomaly(decimal? previousMidPrice, decimal? midPrice, string asset)
            {
                if (previousMidPrice == null) return null;
                if (midPrice == null) return null;

                if (midPrice / previousMidPrice > 10M || previousMidPrice / midPrice > 10M)
                {
                    return $"Found anomaly, orderbook {asset} skipped. Current midPrice is " +
                           $"{previousMidPrice}, the new one is {midPrice}";
                }
                else
                {
                    return null;
                }
            }

            return Observable.Create<OrderBook>(async (obs, ct) =>
            {
                decimal? previousMid = null;

                await source.ForEachAsync(orderBook =>
                {
                    var newMid = MidPrice(orderBook.BestAskPrice, orderBook.BestBidPrice);
                    var anomaly = DetectAnomaly(previousMid, newMid, orderBook.Asset);

                    if (anomaly != null)
                    {
                        log.WriteWarning(
                            nameof(DetectAndFilterAnomalies),
                            "", anomaly);
                    }
                    else {
                        previousMid = newMid ?? previousMid;
                        obs.OnNext(orderBook);
                    }
                }, ct);
            });
        }

        public static IObservable<OrderBook> DetectAndFilterAnomalies(
            this IObservable<OrderBook> source,
            ILog log,
            IEnumerable<string> skipAssets)
        {
            var assetsToSkip = new HashSet<string>(skipAssets.Select(x => x.ToUpperInvariant()));

            return source
                .GroupBy(x => x.Asset)
                .SelectMany(group => assetsToSkip.Contains(group.Key.ToUpperInvariant())
                    ? group
                    : group.DetectAndFilterAnomaliesAssumingSingleInstrument(log));
        }

        public static IObservable<T> NeverIfNotEnabled<T>(this IObservable<T> source, bool enabled)
        {
            return enabled ? source : Observable.Never<T>();
        }

        public static IObservable<OrderBook> OnlyWithPositiveSpread(this IObservable<OrderBook> source)
        {
            return source.Where(x => !x.TryDetectNegativeSpread(out _));
        }

        public static IObservable<T> ThrottleEachInstrument<T>(
            this IObservable<T> source,
            Func<T, string> getAsset,
            float maxEventsPerSecond)
        {
            if (maxEventsPerSecond < 0) throw new ArgumentOutOfRangeException(nameof(maxEventsPerSecond));
            if (Math.Abs(maxEventsPerSecond) < 0.01) return source;

            return source
                .GroupBy(getAsset)
                .Select(grouped => grouped.Sample(TimeSpan.FromSeconds(1) / maxEventsPerSecond))
                .Merge();
        }

        public static IObservable<T> DistinctEveryInstrument<T>(this IObservable<T> source, Func<T, string> getAsset)
        {
            return source.GroupBy(getAsset).Select(x => x.DistinctUntilChanged()).Merge();
        }

        public static IObservable<Unit> PublishToRmq<T>(
            this IObservable<T> source,
            string connectionString,
            string exchanger,
            ILog log)
        {
            const string prefix = "lykke.";

            if (exchanger.StartsWith(prefix)) exchanger = exchanger.Substring(prefix.Length);

            var settings = RabbitMqSubscriptionSettings.CreateForPublisher(
                connectionString,
                exchanger);

            settings.IsDurable = true;

            var connection
                = new RabbitMqPublisher<T>(settings)
                    .SetLogger(log)
                    .SetSerializer(new JsonMessageSerializer<T>())
                    .SetPublishStrategy(new DefaultFanoutPublishStrategy(settings))
                    .PublishSynchronously()
                    .Start();

            return source.SelectMany(async x =>
            {
                await connection.ProduceAsync(x);
                return Unit.Default;
            });
        }

        public static IObservable<Unit> PublishMetrics(this IObservable<OrderBook> source)
        {
            return source.SelectMany(x =>
            {
                InternalMetrics.OrderBookOutCount
                    .WithLabels(x.Asset)
                    .Inc();

                InternalMetrics.OrderBookOutDelayMilliseconds
                    .WithLabels(x.Asset)
                    .Set((DateTime.UtcNow - x.Timestamp).TotalMilliseconds);

                return Task.FromResult(Unit.Default);
            });
        }

        public static IObservable<Unit> PublishMetrics(this IObservable<TickPrice> source)
        {
            return source.SelectMany(x =>
            {
                InternalMetrics.QuoteOutCount
                    .WithLabels(x.Asset)
                    .Inc();

                InternalMetrics.QuoteOutSidePrice
                    .WithLabels(x.Asset, "ask")
                    .Set((double) x.Ask);

                InternalMetrics.QuoteOutSidePrice
                    .WithLabels(x.Asset, "bid")
                    .Set((double) x.Bid);

                return Task.FromResult(Unit.Default);
            });
        }

        public static IObservable<T> ReportErrors<T>(this IObservable<T> source, string process, ILog log)
        {
            return source.Do(_ => { }, err => log.WriteWarning(process, "", "", err));
        }

        public static IObservable<Unit> ReportStatistics<T>(
            this IObservable<T> source,
            TimeSpan window,
            ILog log,
            string format = "Entities registered in the last {0}: {1}")
        {
            return source
                .WindowCount(window)
                .Sample(window)
                .Do(x => log.WriteInfo(nameof(ReportStatistics), "", string.Format(format, window, x)))
                .Select(_ => Unit.Default);
        }
    }
}
