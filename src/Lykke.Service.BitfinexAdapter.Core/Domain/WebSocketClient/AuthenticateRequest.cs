﻿using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace Lykke.Service.BitfinexAdapter.Core.Domain.WebSocketClient
{
    public sealed class AuthenticateRequest : SubscribeRequest
    {
        private static long _nonceSeed;

        [JsonProperty("apiKey")]
        public string ApiKey { get; set; }

        [JsonProperty("authSig")]
        public string AuthSig { get; set; }

        [JsonProperty("authNonce")]
        public string AuthNonce { get; set; }

        [JsonProperty("authPayload")]
        public string AuthPayload { get; set; }

        public static AuthenticateRequest BuildRequest(string apiKey, string apiSecret)
        {
            var nonce = DateTime.UtcNow.Ticks + Interlocked.Increment(ref _nonceSeed);
            var payLoad = "AUTH" + nonce;
            var sig = GetHexHashSignature(payLoad, apiSecret);

            return new AuthenticateRequest
            {
                ApiKey = apiKey,
                AuthSig = sig,
                AuthNonce = nonce.ToString(),
                AuthPayload = payLoad,
                Event = "auth"
            };
        }

        private static string GetHexHashSignature(string payload, string apiSecret)
        {
            var hmac = new HMACSHA384(Encoding.UTF8.GetBytes(apiSecret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}
