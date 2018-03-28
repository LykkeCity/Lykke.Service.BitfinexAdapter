using Common.Log;
using Lykke.Service.BitfinexAdapter.Authentication;
using Lykke.Service.BitfinexAdapter.Core.Domain.Settings;
using Lykke.Service.BitfinexAdapter.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lykke.Service.BitfinexAdapter.Controllers.Api
{
    [ApiKeyAuth]
    public class SpotController : BaseApiController
    {
        public SpotController(BitfinexAdapterSettings configuration, ILog log) : base(configuration, log)
        {
        }

        /// <summary>
        /// See your wallet balances 
        /// </summary>
        [SwaggerOperation("GetWalletBalances")]
        [HttpGet("wallet/balances")]
        [ProducesResponseType(typeof(IEnumerable<WalletBalanceModel>), 200)]
        public async Task<IActionResult> GetWalletBalances()
        {
            return Ok((await GetAuthenticatedExchange().GetWalletBalances(TimeSpan.FromSeconds(DefaultTimeOutSeconds))).Select(m=>m.ToApiModel()));
        }
    }
}
