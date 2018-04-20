using Newtonsoft.Json;

namespace Lykke.Service.BitfinexAdapter.Core.Domain.RestClient
{
    public class BitfinexPostBase
    {
        [JsonProperty("request")]
        public string Request { get; set; }

        [JsonProperty("nonce")]
        public string Nonce { get; set; }
    }

}
