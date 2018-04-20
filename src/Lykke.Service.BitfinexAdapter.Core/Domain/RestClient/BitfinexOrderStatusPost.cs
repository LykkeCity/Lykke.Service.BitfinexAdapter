using Newtonsoft.Json;

namespace Lykke.Service.BitfinexAdapter.Core.Domain.RestClient
{
    public sealed class BitfinexOrderStatusPost : BitfinexPostBase
   {
      /// <summary>
      /// This class can be used to send a cancel message in addition to 
      /// retrieving the current status of an order.
      /// </summary>
      [JsonProperty("order_id")]
      public long OrderId { get; set; }
   }
}
