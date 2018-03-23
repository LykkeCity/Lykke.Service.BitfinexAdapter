using Common.Log;
using Lykke.Service.BitfinexAdapter.Authentication;
using Lykke.Service.BitfinexAdapter.Core.Domain.Settings;
using Lykke.Service.BitfinexAdapter.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.BitfinexAdapter.Controllers.Api
{
    [ApiKeyAuth]
    public class BalanceController : BaseApiController
    {
        public BalanceController(BitfinexAdapterSettings configuration, ILog log) : base(configuration, log)
        {
        }

        /// <summary>
        /// Returns full balance information on the exchange
        /// </summary>
        [SwaggerOperation("GetTradeBalance")]
        [HttpGet("tradeBalance")]
        [ProducesResponseType(typeof(IReadOnlyCollection<TradeBalanceModel>), 200)]
        public async Task<IActionResult> GetTradeBalance()
        {
            return Ok(await GetAuthenticatedExchange().GetTradeBalances(TimeSpan.FromSeconds(DefaultTimeOutSeconds)));
        }
    }
}
