using System;
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
