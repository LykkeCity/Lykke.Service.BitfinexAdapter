using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Common.Log;
using Lykke.Common.ExchangeAdapter;
using Lykke.Common.ExchangeAdapter.Contracts;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.RabbitMqBroker.Subscriber;

namespace Lykke.Service.BitfinexAdapter.Services.OrderBooks
{
    public static class OrderBookPipelineExtensions
    {
        private static IObservable<OrderBook> DetectAndFilterAnomaliesAssumingSingleInstrument(
            this IObservable<OrderBook> source,
            ILog log)
        {
            decimal? MidPrice(IEnumerable<OrderBookItem> orders)
            {
                var prices = orders.Select(x => x.Price).ToArray();
                if (!prices.Any()) return null;
                return (prices.Min() + prices.Max()) / 2M;
            }

            string DetectAnomaly(decimal? previousMidPrice, decimal? midPrice, string side, string asset)
            {
                if (previousMidPrice == null) return null;
                if (midPrice == null) return null;

                if (midPrice / previousMidPrice > 10M || previousMidPrice / midPrice > 10M)
                {
                    return $"Found anomaly, orderbook {asset} skipped. Current {side} midPrice is " +
                           $"{previousMidPrice}, the new one is {midPrice}";
                }
                else
                {
                    return null;
                }
            }

            return Observable.Create<OrderBook>(async (obs, ct) =>
            {
                decimal? previousBid = null;
                decimal? previousAsk = null;

                await source.ForEachAsync(orderBook =>
                {
                    var newAskMidPrice = MidPrice(orderBook.Asks);
                    var askAnomaly = DetectAnomaly(previousAsk, newAskMidPrice, "ask", orderBook.Asset);

                    var newBidMidPrice = MidPrice(orderBook.Bids);
                    var bidAnomaly = DetectAnomaly(previousBid, newBidMidPrice, "bid", orderBook.Asset);

                    if (askAnomaly != null)
                    {
                        log.WriteWarning(
                            nameof(DetectAndFilterAnomalies),
                            "", askAnomaly);
                    }
                    else if (bidAnomaly != null)
                    {
                        log.WriteWarning(
                            nameof(DetectAndFilterAnomalies),
                            "", bidAnomaly);
                    }
                    else
                    {
                        previousAsk = newAskMidPrice ?? previousAsk;
                        previousBid = newBidMidPrice ?? previousBid;
                        obs.OnNext(orderBook);
                    }
                }, ct);
            });
        }

        public static IObservable<OrderBook> DetectAndFilterAnomalies(
            this IObservable<OrderBook> source,
            ILog log)
        {
            return source
                .GroupBy(x => x.Asset)
                .SelectMany(x => x.DetectAndFilterAnomaliesAssumingSingleInstrument(log));
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
