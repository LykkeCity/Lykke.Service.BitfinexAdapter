using Common.Log;
using Lykke.Service.BitfinexAdapter.Authentication;
using Lykke.Service.BitfinexAdapter.Core.Domain.Exceptions;
using Lykke.Service.BitfinexAdapter.Core.Domain.Settings;
using Lykke.Service.BitfinexAdapter.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Lykke.Service.BitfinexAdapter.Controllers.Api
{
    [ApiKeyAuth]
    public class OrdersController : BaseApiController
    {
        public OrdersController(BitfinexAdapterSettings configuration, ILog log) : base(configuration, log)
        {
        }

        /// <summary>
        /// View your active orders
        /// </summary>
        [SwaggerOperation("GetAllActiveOrders")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<OrderModel>), 200)]
        public async Task<IActionResult> GetAllActiveOrders()
        {
            var orders = await GetAuthenticatedExchange().GetOpenOrders(TimeSpan.FromSeconds(DefaultTimeOutSeconds));
            return Ok(orders.Select(s => s.ToApiModel())); 
        }

        /// <summary>
        /// Get order by Id
        /// </summary>
        [SwaggerOperation("GetOrder")]
        [HttpGet("id")]
        [ProducesResponseType(typeof(OrderModel), 200)]
        [ProducesResponseType((int) HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetOrder(long id)
        {
            try
            {
                var order = await GetAuthenticatedExchange().GetOrder(id, TimeSpan.FromSeconds(DefaultTimeOutSeconds));
                return Ok(order.ToApiModel());
            }
            catch (ApiException e)
            {
                if (e.ApiStatusCode == HttpStatusCode.BadRequest)
                {
                    return BadRequest(e.Message);
                }
                if (e.ApiStatusCode == HttpStatusCode.NotFound)
                {
                    return NotFound();
                }
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Cancel order 
        /// </summary>
        [SwaggerOperation("CancelOrder")]
        [HttpPost("cancel")]
        [ProducesResponseType(typeof(long), 200)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CancelLimitOrder(long orderId)
        {
            try
            {
                var result = await GetAuthenticatedExchange().CancelOrder(orderId, TimeSpan.FromSeconds(DefaultTimeOutSeconds));
                return Ok(result);
            }
            catch (ApiException e)
            {
                if (e.ApiStatusCode == HttpStatusCode.BadRequest || e.ApiStatusCode == HttpStatusCode.NotFound)
                {
                    return BadRequest(e.Message);
                }
                return StatusCode((int) HttpStatusCode.InternalServerError);
            }
        }
    }
}
