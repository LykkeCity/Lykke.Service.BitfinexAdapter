using System;
using System.Net;
using System.Runtime.Serialization;

namespace Lykke.Service.BitfinexAdapter.Core.Domain.Exceptions
{
    [Serializable]
    public class ApiException : Exception
    {
        public HttpStatusCode ApiStatusCode { get; }


        public ApiException()
        {
        }

        public ApiException(string message) : base(message)
        {
        }

        public ApiException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ApiException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ApiException(string message, HttpStatusCode apiStatusCode) : base(message)
        {
            ApiStatusCode = apiStatusCode;
        }
    }
}
