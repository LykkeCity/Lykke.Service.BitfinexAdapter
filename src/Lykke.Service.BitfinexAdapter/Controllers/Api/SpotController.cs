using Common.Log;
using Lykke.Service.BitfinexAdapter.Authentication;
using Lykke.Service.BitfinexAdapter.Core.Domain.Exceptions;
using Lykke.Service.BitfinexAdapter.Core.Domain.Settings;
using Lykke.Service.BitfinexAdapter.Models;
using Lykke.Service.BitfinexAdapter.Models.LimitOrders;
using Lykke.Service.BitfinexAdapter.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Lykke.Service.BitfinexAdapter.Controllers.Api
{
    [ApiKeyAuth]
    [Route("spot")]
    public class SpotController : BaseApiController
    {
        public SpotController(BitfinexAdapterSettings configuration, ILog log) : base(configuration, log)
        {
        }

        /// <summary>
        /// See your wallet balances 
        /// </summary>
        [SwaggerOperation("GetWalletBalances")]
        [HttpGet("getWallets")]
        [ProducesResponseType(typeof(GetWalletsResponse), 200)]
        public async Task<IActionResult> GetWalletBalances()
        {
            return Ok(new GetWalletsResponse { Wallets = (await GetAuthenticatedExchange().GetWalletBalances(TimeSpan.FromSeconds(DefaultTimeOutSeconds))).Select(m => m.ToApiModel()) }  );
        }

        /// <summary>
        /// Get all limit order 
        /// </summary>
        [SwaggerOperation("GetLimitOrders")]
        [HttpGet("getLimitOrders")]
        [ProducesResponseType(typeof(GetLimitOrdersResponse), 200)]
        public async Task<IActionResult> GetLimitOrders(string orderIds, string instruments)
        {
            var orderIdsParsed = orderIds?.Split(",").Select(s =>
            {
                if (long.TryParse(s.Trim(), out var parsed)) { return parsed; }
                return 0;
            }).Where(s => s != 0).ToList();


            var orders = await GetAuthenticatedExchange().GetLimitOrders(instruments?.Split(",").Select(s => s.Trim()).ToList(), orderIdsParsed, false, TimeSpan.FromSeconds(DefaultTimeOutSeconds));
            return Ok(new GetLimitOrdersResponse {Orders = orders.Select(s => s.ToApiModel()).ToList() });
        }

        /// <summary>
        /// Get order by Id
        /// </summary>
        [SwaggerOperation("LimitOrderStatus")]
        [HttpGet("limitOrderStatus")]
        [ProducesResponseType(typeof(OrderModel), 200)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> LimitOrderStatus(long orderId)
        {
            return await GetOrder(orderId);
        }

        private async Task<IActionResult> GetOrder(long orderId)
        {
            try
            {
                var order = await GetAuthenticatedExchange().GetOrder(orderId, TimeSpan.FromSeconds(DefaultTimeOutSeconds));
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
        /// Get order by Id
        /// </summary>
        [SwaggerOperation("MarketOrderStatus")]
        [HttpGet("marketOrderStatus")]
        [ProducesResponseType(typeof(OrderModel), 200)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> MarketOrderStatus(long orderId)
        {
            return await GetOrder(orderId);
        }

        /// <summary>
        /// Create Limit order 
        /// </summary>
        [SwaggerOperation("CreateLimitOrder")]
        [HttpPost("createLimitOrder")]
        [ProducesResponseType(typeof(OrderIdResponse), 200)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateLimitOrder([FromBody]LimitOrderRequest request)
        {
            try
            {
                var result = await GetAuthenticatedExchange().AddOrderAndWaitExecution(request.ToLimitOrder(false), TimeSpan.FromSeconds(DefaultTimeOutSeconds));
                return Ok(new OrderIdResponse {OrderId = result.ExchangeOrderId.ToString() });
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
        /// Cancel order 
        /// </summary>
        [SwaggerOperation("CancelOrder")]
        [HttpPost("cancelOrder")]
        [ProducesResponseType(typeof(CancelLimitOrderResponse), 200)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CancelLimitOrder([FromBody]CancelLimitOrderRequest request)
        {
            try
            {
                var result = await GetAuthenticatedExchange().CancelOrder(request.OrderId, TimeSpan.FromSeconds(DefaultTimeOutSeconds));
                return Ok(new CancelLimitOrderResponse {OrderId = result } );
            }
            catch (ApiException e)
            {
                if (e.ApiStatusCode == HttpStatusCode.BadRequest || e.ApiStatusCode == HttpStatusCode.NotFound)
                {
                    return BadRequest(e.Message);
                }
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }


        /// <summary>
        /// Replacel limit order. Cancel one and create new. 
        /// </summary>
        [SwaggerOperation("ReplaceLimitOrder")]
        [HttpPost("replaceLimitOrder")]
        [ProducesResponseType(typeof(OrderIdResponse), 200)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ReplaceLimitOrder([FromBody]ReplaceLimitOrderRequest request)
        {
            try
            {
                var result = await GetAuthenticatedExchange().AddOrderAndWaitExecution(request.ToLimitOrder(false), TimeSpan.FromSeconds(DefaultTimeOutSeconds), request.OrderIdToCancel);
                return Ok(new OrderIdResponse {OrderId = result.ExchangeOrderId.ToString() });
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
        [HttpPost("createMarketOrder")]
        [ProducesResponseType(typeof(OrderIdResponse), 200)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateMarketOrder([FromBody]MarketOrderRequest request)
        {
            try
            {
                var result = await GetAuthenticatedExchange().AddOrderAndWaitExecution(request.ToMarketOrder(false), TimeSpan.FromSeconds(DefaultTimeOutSeconds));
                return Ok(new OrderIdResponse {OrderId = result.ExchangeOrderId.ToString() } );
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

        /// <summary>
        /// View your inactive (history) orders
        /// </summary>
        [SwaggerOperation("GetOrdersHistory")]
        [HttpGet("getOrdersHistory")]
        [ProducesResponseType(typeof(GetOrdersHistoryResponse), 200)]
        public async Task<IActionResult> GetOrdersHistory()
        {
            var orders = await GetAuthenticatedExchange().GetOrdersHistory(TimeSpan.FromSeconds(DefaultTimeOutSeconds));
            return Ok( new GetOrdersHistoryResponse {Orders = orders.Select(s => s.ToApiModel()).ToList()});
        }
    }
}
