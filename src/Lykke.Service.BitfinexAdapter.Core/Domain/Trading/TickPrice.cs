using Newtonsoft.Json;
using System;

namespace Lykke.Service.BitfinexAdapter.Core.Domain.Trading
{
    public class TickPrice
    {
        [JsonConstructor]
        public TickPrice(Instrument instrument, DateTime time, decimal ask, decimal bid)
        {
            Asset = instrument.Name;

            Time = time;
            Ask = ask;
            Bid = bid;
        }

        [JsonProperty("source")]
        public readonly string Source = Constants.BitfinexExchangeName;

        [JsonProperty("asset")]
        public string Asset { get; }

        [JsonProperty("timestamp")]
        public DateTime Time { get; }

        [JsonProperty("ask")]
        public decimal Ask { get; }

        [JsonProperty("bid")]
        public decimal Bid { get; }

        public override string ToString()
        {
            return $"TickPrice for {Asset}: Time={Time}, Ask={Ask}, Bid={Bid}";
        }
    }
}
