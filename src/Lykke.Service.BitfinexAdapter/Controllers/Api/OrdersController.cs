using Common.Log;
using Lykke.Service.BitfinexAdapter.Authentication;
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
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetAllActiveOrders()
        {
            var orders = await GetAuthenticatedExchange().GetOpenOrders(TimeSpan.FromSeconds(DefaultTimeOutSeconds));
            if (orders == null)
            {
                return NotFound();
            }
            return Ok(orders.Select(s => s.ToApiModel()));
        }

        /// <summary>
        /// Get order by Id
        /// </summary>
        [SwaggerOperation("GetOrder")]
        [HttpGet("id")]
        [ProducesResponseType(typeof(OrderModel), 200)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetOrder(long id)
        {
            var order = await GetAuthenticatedExchange().GetOrder(id, TimeSpan.FromSeconds(DefaultTimeOutSeconds));
            if (order.ExchangeOrderId == 0)
            {
                return NotFound();
            }
            return Ok(order);
        }

        /// <summary>
        /// Cancel order 
        /// </summary>
        [SwaggerOperation("CancelOrder")]
        [HttpPost("cancel")]
        [ProducesResponseType(typeof(long), 200)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> CancelLimitOrder(long orderId)
        {
            var result = await GetAuthenticatedExchange().CancelOrder(orderId, TimeSpan.FromSeconds(DefaultTimeOutSeconds));
            if (result == 0)
            {
                return NotFound();
            }
            return Ok(result);
        }


    }
}
