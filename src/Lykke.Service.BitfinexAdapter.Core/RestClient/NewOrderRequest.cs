using System;

namespace Lykke.Service.BitfinexAdapter.Core.RestClient
{
    public class NewOrderRequest
    {
        public long OrderIdToReplace { get; set; }
        public string Symbol { get; set; }
        public decimal Аmount { get; set; }
        public decimal Price { get; set; }
        public string Side { get; set; }
        public string Type { get; set; }

        public override string ToString()
        {
            return String.Format("Symbol {0}, Аmount {1}, Price {2}, Side {3}, Type {4}, {5} ", Symbol, Аmount, Price, Side, Type, OrderIdToReplace > 0 ? $"OrderIdToReplace {OrderIdToReplace}" : ""); 
        }
    }
}
