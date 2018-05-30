using Common.Log;
using Lykke.Service.BitfinexAdapter.Core.Domain.Settings;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.BitfinexAdapter.AzureRepositories;

namespace Lykke.Service.BitfinexAdapter.Controllers.Api
{
    [Route("exchange")]
    public class ExchangeController : BaseApiController
    {
        public ExchangeController(BitfinexAdapterSettings settings, ILimitOrderRepository lor, ILog log)
            : base(settings, lor, log)
        {
        }

        /// <summary>
        /// Get current list of supported instruments
        /// </summary>
        [SwaggerOperation("GetSupportedInstruments")]
        [HttpGet("symbols")]
        [ProducesResponseType(typeof(IEnumerable<string>), 200)]
        public async Task<IActionResult> GetSupportedInstruments()
        {
            return Ok(_configuration.UseSupportedCurrencySymbolsAsFilter ? _configuration.SupportedCurrencySymbols.Select(s => s.ExchangeSymbol).ToList() : (await GetUnAuthenticatedExchange().GetAllExchangeInstruments(TimeSpan.FromSeconds(DefaultTimeOutSeconds)) ).ToList() );
        }
    }
}
