using Newtonsoft.Json;
using System.Net;

namespace Lykke.Service.BitfinexAdapter.Core.Domain.RestClient
{
    public sealed class Error
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("httpStatusCode")]
        public HttpStatusCode HttpApiStatusCode { get; set; }
    }

}
