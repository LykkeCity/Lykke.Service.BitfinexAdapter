using Newtonsoft.Json;

namespace Lykke.Service.BitfinexAdapter.Core.Domain.RestClient
{
    public class BitfinexGetBase
    {
        [JsonProperty("request")]
        public string Request { get; set; }
    }
}
