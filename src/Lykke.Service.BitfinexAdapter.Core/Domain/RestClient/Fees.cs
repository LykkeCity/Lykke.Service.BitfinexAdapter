using System.Collections.Generic;
using Newtonsoft.Json;

namespace Lykke.Service.BitfinexAdapter.Core.Domain.RestClient
{
    public sealed class Fees
    {
        [JsonProperty("withdraw")]
        public IReadOnlyDictionary<string, decimal> Withdraw { get; set; }
    }
}
