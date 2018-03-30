using Lykke.Service.BitfinexAdapter.Core.Domain.Trading.Enums;
using Newtonsoft.Json;
using System;

namespace Lykke.Service.BitfinexAdapter.Core.Domain.Trading
{
    public class TradingSignal
    {
        [JsonConstructor]
        public TradingSignal(
            Instrument instrument,
            string orderId, OrderCommand command, TradeSide tradeSide, decimal? price, decimal volume, bool isMarginOrder, DateTime time, OrderType orderType = OrderType.Market)
        {
            Instrument = instrument;

            OrderId = orderId;
            Command = command;

            TradeSide = tradeSide;
            Price = price;
            Volume = volume;
            Time = time;
            OrderType = orderType;
            IsMarginOrder = isMarginOrder;
        }

        public bool IsMarginOrder { get; }

        public Instrument Instrument { get; }

        public DateTime Time { get; }

        public OrderType OrderType { get; }

        public TradeSide TradeSide { get; }

        public decimal? Price { get; }

        public decimal Volume { get; }

        public string OrderId { get; }

        public OrderCommand Command { get; }

        public override string ToString()
        {
            return $"Id: {OrderId}, Time: {Time}, Instrument: {Instrument}, Command: {Command}, TradeSide: {TradeSide}, Price: {Price}, Count: {Volume}, OrderType: {OrderType}, IsMargin {IsMarginOrder}";
        }

        public bool IsTimeInThreshold(TimeSpan threshold)
        {
            var now = DateTime.UtcNow;

            return Math.Abs(now.Ticks - Time.Ticks) <= threshold.Ticks;
        }
    }
}
