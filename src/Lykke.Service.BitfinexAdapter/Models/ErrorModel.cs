using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace Lykke.Service.BitfinexAdapter.Models
{
    public class ErrorModel
    {
        [JsonProperty("errorMessage")]
        public string ErrorMessage { get; set; }
        [JsonProperty("errorCode")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ApiErrorCode ErrorCode { get; set; }
        [JsonProperty("modelErrors")]
        public Dictionary<string, List<string>> ModelErrors { get; set; }

        public ErrorModel(string message, ApiErrorCode code, Dictionary<string, List<string>> modelErrors = null)
        {
            ErrorMessage = message;
            ErrorCode = code;
            ModelErrors = modelErrors;
        }
    }
}
