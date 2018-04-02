﻿using Common.Log;
using Lykke.Service.BitfinexAdapter.Core.Domain.JsonConverters;
using Lykke.Service.BitfinexAdapter.Core.Domain.RestClient;
using Lykke.Service.BitfinexAdapter.Core.Domain.Trading;
using Lykke.Service.BitfinexAdapter.Core.Utils;
using Microsoft.Rest;
using Microsoft.Rest.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lykke.Service.BitfinexAdapter.Core.RestClient
{
    public sealed class BitfinexApi : ServiceClient<BitfinexApi>, IBitfinexApi
    {
        private const string BalanceRequestUrl = @"/v1/balances";
        private const string NewOrderRequestUrl = @"/v1/order/new";
        private const string OrderStatusRequestUrl = @"/v1/order/status";
        private const string OrderCancelRequestUrl = @"/v1/order/cancel";
        private const string OrderCancelWithReplaceRequestUrl = @"/v1/order/cancel/replace";

        private const string ActiveOrdersRequestUrl = @"/v1/orders";
        private const string ActivePositionsRequestUrl = @"/v1/positions";
        private const string MarginInfoRequstUrl = @"/v1/margin_infos";
        private const string AllSymbolsRequestUrl = @"/v1/symbols";


        private const string BaseBitfinexUrl = @"https://api.bitfinex.com";

        private const string Exchange = "bitfinex";

        private readonly ILog _log;


        public Uri BaseUri { get; set; }

        private readonly ServiceClientCredentials _credentials;

        /// <summary>
        /// Gets or sets json serialization settings.
        /// </summary>
        public JsonSerializerSettings SerializationSettings { get; private set; }

        /// <summary>
        /// Gets or sets json deserialization settings.
        /// </summary>
        public JsonSerializerSettings DeserializationSettings { get; private set; }

        public BitfinexApi(ServiceClientCredentials credentials, ILog log)
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

        public async Task<object> AddOrderAsync(string symbol, decimal amount, decimal price, string side, string type, CancellationToken cancellationToken = default)
        {
            var newOrder = new BitfinexNewOrderPost
            {
                Symbol = symbol,
                Amount = amount,
                Price = price,
                Exchange = Exchange,
                Side = side,
                Type = type,
                Request = NewOrderRequestUrl,
                Nonce = UnixTimeConverter.UnixTimeStampUtc().ToString()
            };

            var response = await GetRestResponse<Order>(newOrder, cancellationToken);

            return response;
        }

        public async Task<object> ReplaceOrderAsync(long orderIdToReplace, string symbol, decimal amount, decimal price, string side, string type, CancellationToken cancellationToken = default)
        {
            var newOrder = new BitfinexReplaceOrderPost()
            {
                OrderIdToReplace = orderIdToReplace,
                Symbol = symbol,
                Amount = amount,
                Price = price,
                Exchange = Exchange,
                Side = side,
                Type = type,
                Request = OrderCancelWithReplaceRequestUrl,
                Nonce = UnixTimeConverter.UnixTimeStampUtc().ToString()
            };

            var response = await GetRestResponse<Order>(newOrder, cancellationToken);

            return response;
        }

        public async Task<object> CancelOrderAsync(long orderId, CancellationToken cancellationToken = default)
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

        public async Task<object> GetActiveOrdersAsync(CancellationToken cancellationToken = default)
        {
            var activeOrdersPost = new BitfinexPostBase
            {
                Request = ActiveOrdersRequestUrl,
                Nonce = UnixTimeConverter.UnixTimeStampUtc().ToString()
            };

            var response = await GetRestResponse<IReadOnlyList<Order>>(activeOrdersPost, cancellationToken);

            return response;
        }


        public async Task<object> GetOrderStatusAsync(long orderId, CancellationToken cancellationToken = default)
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


        public async Task<object> GetWalletBalancesAsync(CancellationToken cancellationToken = default)
        {
            var balancePost = new BitfinexPostBase();
            balancePost.Request = BalanceRequestUrl;
            balancePost.Nonce = UnixTimeConverter.UnixTimeStampUtc().ToString();

            var response = await GetRestResponse<IReadOnlyList<WalletBalance>>(balancePost, cancellationToken);

            return response;
        }

        public async Task<object> GetMarginInformationAsync(CancellationToken cancellationToken = default)
        {
            var marginPost = new BitfinexPostBase
            {
                Request = MarginInfoRequstUrl,
                Nonce = UnixTimeConverter.UnixTimeStampUtc().ToString()
            };


            var response = await GetRestResponse<IReadOnlyList<MarginInfo>>(marginPost, cancellationToken);

            return response;
        }

        public async Task<object> GetActivePositionsAsync(CancellationToken cancellationToken = default)
        {
            var activePositionsPost = new BitfinexPostBase
            {
                Request = ActivePositionsRequestUrl,
                Nonce = UnixTimeConverter.UnixTimeStampUtc().ToString()
            };

            var response = await GetRestResponse<IReadOnlyList<Position>>(activePositionsPost, cancellationToken);

            return response;
        }

        public async Task<object> GetAllSymbolsAsync(CancellationToken cancellationToken = default)
        {
            var response = await GetRestResponse<IReadOnlyList<string>>(new BitfinexGetBase { Request = AllSymbolsRequestUrl }, cancellationToken);

            return response;
        }

        private async Task<object> GetRestResponse<T>(BitfinexPostBase obj, CancellationToken cancellationToken)
        {
            using (var request = await GetRestRequest(obj, cancellationToken))
            {
                return await SendHttpRequestAndGetResponse<T>(request, cancellationToken);
            }
        }

        private async Task<object> GetRestResponse<T>(BitfinexGetBase obj, CancellationToken cancellationToken)
        {
            using (var request = GetRestRequest(obj))
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


        private async Task<object> SendHttpRequestAndGetResponse<T>(HttpRequestMessage request, CancellationToken cancellationToken)
        {
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

        private async Task<object> CheckError<T>(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync(), DeserializationSettings);
            }

            var error = JsonConvert.DeserializeObject<Error>(await response.Content.ReadAsStringAsync(), DeserializationSettings);
            error.HttpApiStatusCode = response.StatusCode;
            return error;
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