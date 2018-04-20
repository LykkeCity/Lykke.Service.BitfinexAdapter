using System;

namespace Lykke.Service.BitfinexAdapter.Core.Domain.Exceptions
{
    public class OrderBookInconsistencyException : ApiException
    {
        public OrderBookInconsistencyException(string message) : base(message)
        {
        }

        public OrderBookInconsistencyException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
