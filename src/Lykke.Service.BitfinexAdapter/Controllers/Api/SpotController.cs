using Common.Log;
using Lykke.Service.BitfinexAdapter.Authentication;
using Lykke.Service.BitfinexAdapter.Core.Domain.Exceptions;
using Lykke.Service.BitfinexAdapter.Core.Domain.Settings;
using Lykke.Service.BitfinexAdapter.Core.Domain.Trading.Enums;
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
        [ProducesResponseType(typeof(ErrorModel), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetWalletBalances()
        {
            try
            {
                return Ok(new GetWalletsResponse { Wallets = (await GetAuthenticatedExchange().GetWalletBalances(TimeSpan.FromSeconds(DefaultTimeOutSeconds))).Select(m => m.ToApiModel()) });
            }
            catch (ApiException e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ErrorModel(e.Message, e.ErrorCode));
            }
        }

        /// <summary>
        /// Get all limit order 
        /// </summary>
        [SwaggerOperation("GetLimitOrders")]
        [HttpGet("getLimitOrders")]
        [ProducesResponseType(typeof(GetLimitOrdersResponse), 200)]
        [ProducesResponseType(typeof(ErrorModel), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetLimitOrders(string orderIds, string instruments)
        {
            try
            {
                var orderIdsParsed = orderIds?.Split(",").Select(s =>
                {
                    if (long.TryParse(s.Trim(), out var parsed)) { return parsed; }
                    return 0;
                }).Where(s => s != 0).ToList();


                var orders = await GetAuthenticatedExchange().GetLimitOrders(instruments?.Split(",").Select(s => s.Trim()).ToList(), orderIdsParsed, false, TimeSpan.FromSeconds(DefaultTimeOutSeconds));
                return Ok(new GetLimitOrdersResponse { Orders = orders.Select(s => s.ToApiModel()).ToList() });
            }
            catch (ApiException e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ErrorModel(e.Message, e.ErrorCode));
            }
        }

        /// <summary>
        /// Get order by Id
        /// </summary>
        [SwaggerOperation("LimitOrderStatus")]
        [HttpGet("limitOrderStatus")]
        [ProducesResponseType(typeof(OrderModel), 200)]
        [ProducesResponseType(typeof(ErrorModel), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorModel), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> LimitOrderStatus(long orderId)
        {
            return await GetOrder(orderId, OrderType.Limit);
        }

        private async Task<IActionResult> GetOrder(long orderId, OrderType orderType = OrderType.Unknown)
        {
            try
            {
                var order = await GetAuthenticatedExchange().GetOrder(orderId, TimeSpan.FromSeconds(DefaultTimeOutSeconds), orderType);
                return Ok(order.ToApiModel());
            }
            catch (ApiException e)
            {
                if (e.ApiStatusCode == HttpStatusCode.BadRequest)
                {
                    return BadRequest(new ErrorModel(e.Message, e.ErrorCode));
                }
                if (e.ApiStatusCode == HttpStatusCode.NotFound)
                {
                    return NotFound(new ErrorModel(e.Message, ApiErrorCode.OrderNotFound));
                }
                return StatusCode((int)HttpStatusCode.InternalServerError, new ErrorModel(e.Message, e.ErrorCode));
            }
        }

        /// <summary>
        /// Get order by Id
        /// </summary>
        [SwaggerOperation("MarketOrderStatus")]
        [HttpGet("marketOrderStatus")]
        [ProducesResponseType(typeof(OrderModel), 200)]
        [ProducesResponseType(typeof(ErrorModel), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorModel), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> MarketOrderStatus(long orderId)
        {
            return await GetOrder(orderId, OrderType.Market);
        }

        /// <summary>
        /// Create Limit order 
        /// </summary>
        [SwaggerOperation("CreateLimitOrder")]
        [HttpPost("createLimitOrder")]
        [ProducesResponseType(typeof(OrderIdResponse), 200)]
        [ProducesResponseType(typeof(ErrorModel), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorModel), (int)HttpStatusCode.InternalServerError)]
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
                    return BadRequest(new ErrorModel(ex.Message, ex.ErrorCode));
                }
                
                return StatusCode((int)HttpStatusCode.InternalServerError, new ErrorModel(ex.Message, ex.ErrorCode));
            }
        }

        /// <summary>
        /// Cancel order 
        /// </summary>
        [SwaggerOperation("CancelOrder")]
        [HttpPost("cancelOrder")]
        [ProducesResponseType(typeof(CancelLimitOrderResponse), 200)]
        [ProducesResponseType(typeof(ErrorModel), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CancelLimitOrder([FromBody]CancelLimitOrderRequest request)
        {
            try
            {
                var result = await GetAuthenticatedExchange().CancelOrder(request.OrderId, TimeSpan.FromSeconds(DefaultTimeOutSeconds));
                return Ok(new CancelLimitOrderResponse {OrderId = result } );
            }
            catch (ApiException e)
            {
                if (e.ErrorCode == ApiErrorCode.OrderNotFound || e.ApiStatusCode == HttpStatusCode.NotFound)
                {
                    return NotFound(new ErrorModel(e.Message, e.ErrorCode));
                }

                if (e.ApiStatusCode == HttpStatusCode.BadRequest)
                {
                    return BadRequest(new ErrorModel(e.Message, e.ErrorCode));
                }
                return StatusCode((int)HttpStatusCode.InternalServerError, new ErrorModel(e.Message, e.ErrorCode));
            }
        }


        /// <summary>
        /// Replace limit order. Cancel one and create new. 
        /// </summary>
        [SwaggerOperation("ReplaceLimitOrder")]
        [HttpPost("replaceLimitOrder")]
        [ProducesResponseType(typeof(OrderIdResponse), 200)]
        [ProducesResponseType(typeof(ErrorModel), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorModel), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorModel), (int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ReplaceLimitOrder([FromBody]ReplaceLimitOrderRequest request)
        {
            try
            {
                var result = await GetAuthenticatedExchange().AddOrderAndWaitExecution(request.ToLimitOrder(false), TimeSpan.FromSeconds(DefaultTimeOutSeconds), request.OrderIdToCancel);
                return Ok(new OrderIdResponse {OrderId = result.ExchangeOrderId.ToString() });
            }
            catch (ApiException e)
            {
                if (e.ErrorCode == ApiErrorCode.OrderNotFound || e.ApiStatusCode == HttpStatusCode.NotFound)
                {
                    return NotFound(new ErrorModel(e.Message, ApiErrorCode.OrderNotFound));
                }

                if (e.ApiStatusCode == HttpStatusCode.BadRequest)
                {
                    return BadRequest(new ErrorModel(e.Message, e.ErrorCode));
                }
                return StatusCode((int)HttpStatusCode.InternalServerError, new ErrorModel(e.Message, e.ErrorCode));
            }
        }

        /// <summary>
        /// Create Market order 
        /// </summary>
        [SwaggerOperation("CreateMarketOrder")]
        [HttpPost("createMarketOrder")]
        [ProducesResponseType(typeof(OrderIdResponse), 200)]
        [ProducesResponseType(typeof(ErrorModel), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorModel), (int)HttpStatusCode.InternalServerError)]
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
                    return BadRequest(new ErrorModel(ex.Message, ex.ErrorCode));
                }
                return StatusCode((int)HttpStatusCode.InternalServerError, new ErrorModel(ex.Message, ex.ErrorCode));
            }
        }

        /// <summary>
        /// View your inactive (history) orders
        /// </summary>
        [SwaggerOperation("GetOrdersHistory")]
        [HttpGet("getOrdersHistory")]
        [ProducesResponseType(typeof(GetOrdersHistoryResponse), 200)]
        [ProducesResponseType(typeof(ErrorModel), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetOrdersHistory()
        {
            try
            {
                var orders = await GetAuthenticatedExchange().GetOrdersHistory(TimeSpan.FromSeconds(DefaultTimeOutSeconds));
                return Ok(new GetOrdersHistoryResponse { Orders = orders.Select(s => s.ToApiModel()).ToList() });
            }
            catch (ApiException e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ErrorModel(e.Message, e.ErrorCode));
            }
        }
    }
}
