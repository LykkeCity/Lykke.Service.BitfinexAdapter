using Common.Log;
using Lykke.Service.BitfinexAdapter.Authentication;
using Lykke.Service.BitfinexAdapter.Core.Domain.Exceptions;
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
        /// Get all limit order 
        /// </summary>
        [SwaggerOperation("GetLimitOrders")]
        [HttpGet("orders/limit")]
        [ProducesResponseType(typeof(IEnumerable<OrderModel>), 200)]
        public async Task<IActionResult> GetLimitOrders(string orderIds, string instruments)
        {
            var orderIdsParsed = orderIds?.Split(",").Select(s =>
            {
                if (long.TryParse(s.Trim(), out var parsed)) { return parsed; }
                return 0;
            }).Where(s => s != 0).ToList();


            var orders = await GetAuthenticatedExchange().GetLimitOrders(instruments?.Split(",").Select(s => s.Trim()).ToList(), orderIdsParsed, false, TimeSpan.FromSeconds(DefaultTimeOutSeconds));
            return Ok(orders.Select(s => s.ToApiModel()));
        }

        /// <summary>
        /// Create Limit order 
        /// </summary>
        [SwaggerOperation("CreateLimitOrder")]
        [HttpPost("order/limit")]
        [ProducesResponseType(typeof(long), 200)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateLimitOrder(LimitOrderRequest request)
        {
            try
            {
                var result = await GetAuthenticatedExchange().AddOrderAndWaitExecution(request.ToLimitOrder(false), TimeSpan.FromSeconds(DefaultTimeOutSeconds));
                return Ok(result.ExchangeOrderId);
            }
            catch (ApiException ex)
            {
                if (ex.ApiStatusCode == HttpStatusCode.BadRequest)
                {
                    return BadRequest(ex.Message);
                }
                return StatusCode((int) HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Replacel limit order. Cancel one and create new. 
        /// </summary>
        [SwaggerOperation("ReplaceLimitOrder")]
        [HttpPost("order/limit/replace")]
        [ProducesResponseType(typeof(long), 200)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ReplaceLimitOrder(ReplaceLimitOrderRequest request)
        {
            try
            {
                if (request.OrderIdToCancel <= 0)
                {
                    return BadRequest("OrderId to replace not specified.");
                }

                var result = await GetAuthenticatedExchange().AddOrderAndWaitExecution(request.ToLimitOrder(false), TimeSpan.FromSeconds(DefaultTimeOutSeconds), request.OrderIdToCancel);
                return Ok(result.ExchangeOrderId);
            }
            catch (ApiException ex)
            {
                if (ex.ApiStatusCode == HttpStatusCode.BadRequest)
                {
                    return BadRequest(ex.Message);
                }
                if (ex.ApiStatusCode == HttpStatusCode.NotFound)
                {
                    return NotFound(ex.Message);
                }
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Create Market order 
        /// </summary>
        [SwaggerOperation("CreateMarketOrder")]
        [HttpPost("order/market")]
        [ProducesResponseType(typeof(long), 200)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateMarketOrder(MarketOrderRequest request)
        {
            try
            {
                var result = await GetAuthenticatedExchange().AddOrderAndWaitExecution(request.ToMarketOrder(false), TimeSpan.FromSeconds(DefaultTimeOutSeconds));
                return Ok(result.ExchangeOrderId);
            }
            catch (ApiException ex)
            {
                if (ex.ApiStatusCode == HttpStatusCode.BadRequest)
                {
                    return BadRequest(ex.Message);
                }
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
