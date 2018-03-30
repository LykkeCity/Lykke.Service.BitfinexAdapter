using Common.Log;
using Lykke.Service.BitfinexAdapter.Authentication;
using Lykke.Service.BitfinexAdapter.Core.Domain.Settings;
using Lykke.Service.BitfinexAdapter.Models;
using Lykke.Service.BitfinexAdapter.Models.LimitOrders;
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

        /// <summary>
        /// Create Limit order 
        /// </summary>
        [SwaggerOperation("CreateLimitOrder")]
        [HttpPost("order/limit")]
        [ProducesResponseType(typeof(long), 200)]
        public async Task<IActionResult> CreateLimitOrder(LimitOrderRequest request)
        {
            var result = await GetAuthenticatedExchange().AddOrderAndWaitExecution(request.ToLimitOrderTradingSignal(false),TimeSpan.FromSeconds(DefaultTimeOutSeconds));
            if (result == null)
            {
                return NotFound(); // TODO: Better to return 500 Internal Server Error
            }

            return Ok(result.ExchangeOrderId);
        }

        /// <summary>
        /// Create Limit order 
        /// </summary>
        [SwaggerOperation("GetLimitOrders")]
        [HttpGet("orders/limit")]
        [ProducesResponseType(typeof(IEnumerable<OrderModel>), 200)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetLimitOrders([FromQuery]long[] Ids, [FromQuery]string[] Instruments  /*[FromBody]OrdersRequestWithFilter ordersFilter*/)  //With HttpPost
        {
            //With HttpPost //var orders = await GetAuthenticatedExchange().GetLimitOrders(ordersFilter.Instruments?.ToList(), ordersFilter.Ids?.ToList(), false, TimeSpan.FromSeconds(DefaultTimeOutSeconds));
            var orders = await GetAuthenticatedExchange().GetLimitOrders(Instruments?.ToList(), Ids?.ToList(), false, TimeSpan.FromSeconds(DefaultTimeOutSeconds));
            if (orders == null)
            {
                return NotFound();
            }

            return Ok(orders.Select(s => s.ToApiModel()));
        }

        public long[] Ids { get; set; }
        public string[] Instruments { get; set; }

        /// <summary>
        /// Replacel limit order. Cancel one and create new. 
        /// </summary>
        [SwaggerOperation("ReplaceLimitOrder")]
        [HttpPost("order/replace")]
        [ProducesResponseType(typeof(long), 200)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ReplaceLimitOrder(ReplaceLimitOrderRequest request)
        {
            var result = await GetAuthenticatedExchange().ReplaceLimitOrder(request.OrderIdToCancel, request.ToLimitOrderTradingSignal(false), TimeSpan.FromSeconds(DefaultTimeOutSeconds));

            if (!result.HasValue)
            {
                return StatusCode(500, "Requested order was cancelled, but an error occured while creating new one.");
            }


            if (result.Value == 0)
            {
                NotFound("Requested order to cancel(replace) was not found. A new order has not been created.");
            }

            return Ok(request.OrderIdToCancel);
        }
    }
}
