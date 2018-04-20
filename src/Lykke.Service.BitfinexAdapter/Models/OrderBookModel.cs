using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Lykke.Service.BitfinexAdapter.Models
{
    public sealed class OrderBookModel
    {
        public OrderBookModel(string source, string assetPairId, IReadOnlyCollection<VolumePriceModel> asks, IReadOnlyCollection<VolumePriceModel> bids, DateTime timestamp)
        {
            Source = source;
            AssetPairId = assetPairId;
            Asks = asks;
            Bids = bids;
            Timestamp = timestamp;
        }

        [JsonProperty("source")]
        public string Source { get; }

        [JsonProperty("asset")]
        public string AssetPairId { get; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; }

        [JsonProperty("asks")]
        public IReadOnlyCollection<VolumePriceModel> Asks { get; }

        [JsonProperty("bids")]
        public IReadOnlyCollection<VolumePriceModel> Bids { get; }

    }
}
