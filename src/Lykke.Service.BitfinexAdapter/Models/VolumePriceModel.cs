using Newtonsoft.Json;
using System;

namespace Lykke.Service.BitfinexAdapter.Models
{
    public sealed class VolumePriceModel
    {
        public VolumePriceModel(decimal price, decimal volume)
        {
            Price = price;
            Volume = Math.Abs(volume);
        }

        [JsonProperty("price")]
        public decimal Price { get; }

        [JsonProperty("volume")]
        public decimal Volume { get; }

    }
}
