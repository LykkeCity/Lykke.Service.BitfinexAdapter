using System;
using System.Net;
using System.Runtime.Serialization;

namespace Lykke.Service.BitfinexAdapter.Core.Domain.Exceptions
{
    [Serializable]
    public class ApiException : Exception
    {
        public HttpStatusCode ApiStatusCode { get; }
        public ApiErrorCode ErrorCode { get; }

        public ApiException()
        {
        }

        public ApiException(string message) : base(message)
        {
        }

        public ApiException(string message, Exception innerException) : base(message, innerException)
        {
            ErrorCode = TryParseBitfinexErrorMessage(message);
        }

        protected ApiException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ApiException(string message, HttpStatusCode apiStatusCode) : base(message)
        {
            ApiStatusCode = apiStatusCode;
            ErrorCode = TryParseBitfinexErrorMessage(message);
        }

        public ApiException(string message, HttpStatusCode apiStatusCode, ApiErrorCode errorCode) : base(message)
        {
            ApiStatusCode = apiStatusCode;
            ErrorCode = errorCode;
        }

        protected ApiErrorCode TryParseBitfinexErrorMessage(string errorMessage)
        {
            if (String.IsNullOrWhiteSpace(errorMessage)) return ApiErrorCode.Unknown;

            if (errorMessage == "Order could not be cancelled.")
                return ApiErrorCode.OrderNotFound;
            if (errorMessage.Contains("price should be a decimal number") || errorMessage == "Price too big")
                return ApiErrorCode.IncorrectPrice;
            if (errorMessage == "Unknown symbol")
                return ApiErrorCode.IncorrectInstrument;
            if (errorMessage == "Order amount must be positive." || errorMessage.Contains("Invalid order: minimum size") || errorMessage.Contains("Invalid order size") || errorMessage.Contains("amount should be a decimal number"))
                return ApiErrorCode.IncorrectAmount;
            if (errorMessage.Contains("not enough exchange balance"))
                return ApiErrorCode.NotEnoughBalance;
            if (errorMessage == "Invalid X-BFX-SIGNATURE." || errorMessage == "Could not find a key matching the given X-BFX-APIKEY.")
                return ApiErrorCode.Unauthorized;
            if (errorMessage == "Ratelimit")
                return ApiErrorCode.RateLimit;

            return ApiErrorCode.Unknown;
        }
    }
}
