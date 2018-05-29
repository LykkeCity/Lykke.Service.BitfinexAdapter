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
using Lykke.Service.BitfinexAdapter.AzureRepositories;

namespace Lykke.Service.BitfinexAdapter.Controllers.Api
{
    [Route("margin")]
    [ApiKeyAuth]
    public class MarginController : BaseApiController
    {
        public MarginController(BitfinexAdapterSettings configuration, LimitOrderRepository lor, ILog log)
            : base(configuration, lor, log)
        {
        }

        /// <summary>
        /// Returns full margin balance information on the exchange
        /// </summary>
        [SwaggerOperation("GetMarginBalances")]
        [HttpGet("balances")]
        [ProducesResponseType(typeof(IEnumerable<MarginBalanceModel>), 200)]
        public async Task<IActionResult> GetMarginBalances()
        {
            return Ok((await GetAuthenticatedExchange().GetMarginBalances(TimeSpan.FromSeconds(DefaultTimeOutSeconds))).Select(m=>m.ToApiModel()).ToList());
        }
    }
}
