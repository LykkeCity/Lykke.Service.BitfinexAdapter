using Lykke.Service.BitfinexAdapter.Core.Domain.Settings;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.BitfinexAdapter.Controllers.Api
{
    public class ExchangeController : BaseApiController
    {
        private readonly BitfinexAdapterSettings _settings;

        public ExchangeController(BitfinexAdapterSettings settings)
        {
            _settings = settings;
        }

        /// <summary>
        /// Get current list of supported instruments
        /// </summary>
        [SwaggerOperation("GetSupportedInstruments")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<string>), 200)]
        public async Task<IActionResult> GetSupportedInstruments()
        {
            return Ok(_settings.SupportedCurrencySymbols); //TODO: return from exchange api if list is empty
        }
    }
}
