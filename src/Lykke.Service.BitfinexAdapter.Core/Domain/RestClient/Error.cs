using Newtonsoft.Json;

namespace Lykke.Service.BitfinexAdapter.Core.Domain.RestClient
{
    public sealed class Error
    {
        [JsonProperty("message")]
        public string Message { get; set; }
    }

}
