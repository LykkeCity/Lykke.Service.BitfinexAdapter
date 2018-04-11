using Newtonsoft.Json;
using System.Collections.Generic;

namespace Lykke.Service.BitfinexAdapter.Models.Responses
{
    public class GetWalletsResponse
    {
        [JsonProperty("wallets")]
        public IEnumerable<WalletBalanceModel> Wallets { get; set; }
    }
}
