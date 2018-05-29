using Common.Log;
using Lykke.Service.BitfinexAdapter.Core.Domain.Exceptions;
using Lykke.Service.BitfinexAdapter.Core.Domain.JsonConverters;
using Lykke.Service.BitfinexAdapter.Core.Domain.RestClient;
using Lykke.Service.BitfinexAdapter.Core.Domain.Trading;
using Lykke.Service.BitfinexAdapter.Core.Utils;
using Microsoft.Rest;
using Microsoft.Rest.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lykke.Common.ExchangeAdapter;
using Newtonsoft.Json.Linq;

namespace Lykke.Service.BitfinexAdapter.Core.RestClient
{
    public sealed class BitfinexApi : ServiceClient<BitfinexApi>
    {
        private const string BalanceRequestUrl = @"/v1/balances";
        private const string NewOrderRequestUrl = @"/v1/order/new";
        private const string OrderStatusRequestUrl = @"/v1/order/status";
        private const string OrderCancelRequestUrl = @"/v1/order/cancel";
        private const string OrderCancelWithReplaceRequestUrl = @"/v1/order/cancel/replace";
        private const string InactiveOrdersRequestUrl = @"/v1/orders/hist";

        private const string ActiveOrdersRequestUrl = @"/v1/orders";
        private const string ActivePositionsRequestUrl = @"/v1/positions";
        private const string MarginInfoRequstUrl = @"/v1/margin_infos";
        private const string AllSymbolsRequestUrl = @"/v1/symbols";


        private const string BaseBitfinexUrl = @"https://api.bitfinex.com";

        private const string Exchange = "bitfinex";

        private readonly ILog _log;

        public Uri BaseUri { get; set; }

        private readonly BitfinexServiceClientCredentials _credentials;

        /// <summary>
        /// Gets or sets json serialization settings.
        /// </summary>
        public JsonSerializerSettings SerializationSettings { get; private set; }

        /// <summary>
        /// Gets or sets json deserialization settings.
        /// </summary>
        public JsonSerializerSettings DeserializationSettings { get; private set; }

        public BitfinexApi(BitfinexServiceClientCredentials credentials, ILog log)
        {
            _credentials = credentials;
            Initialize();
            _log = log;
        }

        private void Initialize()
        {
            BaseUri = new Uri(BaseBitfinexUrl);
            SerializationSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                ContractResolver = new ReadOnlyJsonContractResolver(),
                Converters = new List<JsonConverter>
                {
                    new Iso8601TimeSpanConverter(),
                    new StringDecimalConverter()
                }
            };
            DeserializationSettings = new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                ContractResolver = new ReadOnlyJsonContractResolver(),
                Converters = new List<JsonConverter>
                {
                    new BitfinexDateTimeConverter(),
                    new Iso8601TimeSpanConverter()
                }
            };
        }

        public async Task<Order> AddOrderAsync(NewOrderRequest orderRequest, CancellationToken cancellationToken = default)
        {
            var newOrder = new BitfinexNewOrderPost
            {
                Symbol = orderRequest.Symbol,
                Amount = orderRequest.Аmount,
                Price = orderRequest.Price,
                Exchange = Exchange,
                Side = orderRequest.Side,
                Type = orderRequest.Type,
                Request = NewOrderRequestUrl,
                Nonce = UnixTimeConverter.UnixTimeStampUtc().ToString()
            };

            var response = await GetRestResponse<Order>(newOrder, cancellationToken);

            return response;
        }

        public async Task<Fees> GetFees(CancellationToken ct = default)
        {
            var request = new BitfinexPostBase
            {
                Request = "/v1/account_fees"
            };

            var response = await GetRestResponse<JToken>(request, ct);
            return response.ToObject<Fees>();
        }

        public async Task<Order> ReplaceOrderAsync(NewOrderRequest orderRequest, CancellationToken cancellationToken = default)
        {
            var newOrder = new BitfinexReplaceOrderPost()
            {
                OrderIdToReplace = orderRequest.OrderIdToReplace,
                Symbol = orderRequest.Symbol,
                Amount = orderRequest.Аmount,
                Price = orderRequest.Price,
                Exchange = Exchange,
                Side = orderRequest.Side,
                Type = orderRequest.Type,
                Request = OrderCancelWithReplaceRequestUrl,
                Nonce = UnixTimeConverter.UnixTimeStampUtc().ToString()
            };

            var response = await GetRestResponse<Order>(newOrder, cancellationToken);

            return response;
        }

        public async Task<Order> CancelOrderAsync(long orderId, CancellationToken cancellationToken = default)
        {
            var cancelPost = new BitfinexOrderStatusPost
            {
                Request = OrderCancelRequestUrl,
                Nonce = UnixTimeConverter.UnixTimeStampUtc().ToString(),
                OrderId = orderId
            };

            var response = await GetRestResponse<Order>(cancelPost, cancellationToken);

            return response;
        }

        public async Task<ReadOnlyCollection<Order>> GetActiveOrdersAsync(CancellationToken cancellationToken = default)
        {
            var activeOrdersPost = new BitfinexPostBase
            {
                Request = ActiveOrdersRequestUrl,
                Nonce = UnixTimeConverter.UnixTimeStampUtc().ToString()
            };

            var response = await GetRestResponse<ReadOnlyCollection<Order>>(activeOrdersPost, cancellationToken);

            return response;
        }

        public async Task<ReadOnlyCollection<Order>> GetInactiveOrdersAsync(CancellationToken cancellationToken = default)
        {
            var inactiveOrdersPost = new BitfinexPostBase
            {
                Request = InactiveOrdersRequestUrl,
                Nonce = UnixTimeConverter.UnixTimeStampUtc().ToString()
            };

            var response = await GetRestResponse<ReadOnlyCollection<Order>>(inactiveOrdersPost, cancellationToken);

            return response;
        }


        public async Task<Order> GetOrderStatusAsync(long orderId, CancellationToken cancellationToken = default)
        {
            var orderStatusPost = new BitfinexOrderStatusPost
            {
                Request = OrderStatusRequestUrl,
                Nonce = UnixTimeConverter.UnixTimeStampUtc().ToString(),
                OrderId = orderId
            };

            var response = await GetRestResponse<Order>(orderStatusPost, cancellationToken);

            return response;
        }


        public async Task<ReadOnlyCollection<WalletBalance>> GetWalletBalancesAsync(CancellationToken cancellationToken = default)
        {
            var balancePost = new BitfinexPostBase();
            balancePost.Request = BalanceRequestUrl;
            balancePost.Nonce = UnixTimeConverter.UnixTimeStampUtc().ToString();

            var response = await GetRestResponse<ReadOnlyCollection<WalletBalance>>(balancePost, cancellationToken);

            return response;
        }

        public async Task<ReadOnlyCollection<MarginInfo>> GetMarginInformationAsync(CancellationToken cancellationToken = default)
        {
            var marginPost = new BitfinexPostBase
            {
                Request = MarginInfoRequstUrl,
                Nonce = UnixTimeConverter.UnixTimeStampUtc().ToString()
            };


            var response = await GetRestResponse<ReadOnlyCollection<MarginInfo>>(marginPost, cancellationToken);

            return response;
        }

        public async Task<ReadOnlyCollection<Position>> GetActivePositionsAsync(CancellationToken cancellationToken = default)
        {
            var activePositionsPost = new BitfinexPostBase
            {
                Request = ActivePositionsRequestUrl,
                Nonce = UnixTimeConverter.UnixTimeStampUtc().ToString()
            };

            var response = await GetRestResponse<ReadOnlyCollection<Position>>(activePositionsPost, cancellationToken);

            return response;
        }

        public async Task<ReadOnlyCollection<string>> GetAllSymbolsAsync(CancellationToken cancellationToken = default)
        {
            var response = await GetRestResponse<ReadOnlyCollection<string>>(new BitfinexGetBase { Request = AllSymbolsRequestUrl }, cancellationToken);

            return response;
        }

        private Task<T> GetRestResponse<T>(BitfinexPostBase post, CancellationToken cancellationToken)
        {
            return EpochNonce.Lock(_credentials.ApiKey, async nonce =>
            {
                post.Nonce = nonce.ToString();
                using (var request = await GetRestRequest(post, cancellationToken))
                {
                    return await SendHttpRequestAndGetResponse<T>(request, cancellationToken);
                }
            });
        }

        private async Task<T> GetRestResponse<T>(BitfinexGetBase get, CancellationToken cancellationToken)
        {
            using (var request = GetRestRequest(get))
            {
                return await SendHttpRequestAndGetResponse<T>(request, cancellationToken);
            }
        }

        private async Task<HttpRequestMessage> GetRestRequest(BitfinexPostBase obj, CancellationToken cancellationToken)
        {

            // Create HTTP transport objects
            var httpRequest = new HttpRequestMessage
            {
                Method = new HttpMethod("POST"),
                RequestUri = new Uri(BaseUri, obj.Request)
            };

            var jsonObj = SafeJsonConvert.SerializeObject((object)obj, (JsonSerializerSettings)SerializationSettings);
            httpRequest.Content = new StringContent(jsonObj, Encoding.UTF8, "application/json");

            await _credentials.ProcessHttpRequestAsync(httpRequest, cancellationToken).ConfigureAwait(false);

            return httpRequest;
        }

        private HttpRequestMessage GetRestRequest(BitfinexGetBase obj)
        {

            var httpRequest = new HttpRequestMessage
            {
                Method = new HttpMethod("GET"),
                RequestUri = new Uri(BaseUri, obj.Request)
            };

            return httpRequest;
        }


        private async Task<T> SendHttpRequestAndGetResponse<T>(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var content = request.Content == null ? "<empty>" : await request.Content.ReadAsStringAsync();
            _log.WriteInfo(nameof(BitfinexApi), "", content);
            using (var response = await HttpClient.SendAsync(request, cancellationToken))
            {
                try
                {
                    var responseBody = await CheckError<T>(response);
                    return responseBody;
                }
                catch (Exception e)
                {
                    await _log.WriteErrorAsync(nameof(BitfinexApi), request.RequestUri.AbsoluteUri, e);
                    throw;
                }
            }
        }

        private async Task<T> CheckError<T>(HttpResponseMessage response)
        {
            var responseAsString = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<T>(responseAsString, DeserializationSettings);
            }

            var error = JsonConvert.DeserializeObject<Error>(responseAsString, DeserializationSettings);

            if (error == null)
            {
                throw new ApiException(responseAsString, response.StatusCode);
            }
            throw new ApiException(responseAsString, response.StatusCode);
        }

        private sealed class StringDecimalConverter : JsonConverter
        {
            public override bool CanRead => false;

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(decimal) || objectType == typeof(decimal?);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteValue(((decimal)value).ToString(CultureInfo.InvariantCulture));
            }
        }
    }
}
