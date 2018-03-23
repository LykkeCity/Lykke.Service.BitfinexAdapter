using System.Collections.Generic;

namespace Lykke.Service.BitfinexAdapter.Models
{
    public class ErrorModel
    {
        public string ErrorMessage { get; set; }
        public int ErrorCode { get; set; }
        public Dictionary<string, List<string>> ModelErrors { get; set; }

        public ErrorModel(string message, int code, Dictionary<string, List<string>> modelErrors = null)
        {
            ErrorMessage = message;
            ErrorCode = code;
            ModelErrors = modelErrors;
        }

    }
}
