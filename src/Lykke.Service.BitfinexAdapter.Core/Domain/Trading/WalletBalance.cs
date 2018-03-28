namespace Lykke.Service.BitfinexAdapter.Core.Domain.Trading
{
    public sealed class WalletBalance
    {
        public string Type { get; set; }

        public string Currency { get; set; }

        public decimal Amount { get; set; }

        public decimal Available { get; set; }

       public override string ToString()
       {
          var str = string.Format("Type: {0}, Currency: {1}, Amount: {2}, Available: {3}", Type, Currency, Amount,Available);
          return str;
       }
    }

}
