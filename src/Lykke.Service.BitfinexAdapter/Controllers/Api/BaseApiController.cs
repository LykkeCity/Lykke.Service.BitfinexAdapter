using Common.Log;
using Lykke.Service.BitfinexAdapter.Core.Domain;
using Lykke.Service.BitfinexAdapter.Core.Domain.Settings;
using Lykke.Service.BitfinexAdapter.Logging;
using Lykke.Service.BitfinexAdapter.Services.Exchange;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System;
using System.Security.Authentication;

namespace Lykke.Service.BitfinexAdapter.Controllers.Api
{

    [Produces("application/json")]
    [LoggingAspNetFilter]
    public abstract class BaseApiController : Controller
    {
        protected readonly BitfinexAdapterSettings _configuration;
        protected readonly ILog _log;
        protected int DefaultTimeOutSeconds = 30;

        public BaseApiController(BitfinexAdapterSettings configuration, ILog log)
        {
            _configuration = configuration;
            _log = log;
        }

        protected ExchangeBase GetAuthenticatedExchange()
        {
            StringValues clientXapiKey;

            if (!Request.Headers.TryGetValue(Constants.XApiKeyHeaderName, out clientXapiKey) || 
                !_configuration.Credentials.ContainsKey(clientXapiKey) ||
                String.IsNullOrWhiteSpace(_configuration.Credentials[clientXapiKey].ApiKey) ||
                String.IsNullOrWhiteSpace(_configuration.Credentials[clientXapiKey].ApiSecret))
            {
                throw new AuthenticationException(Constants.AuthenticationError);
            }

            return new BitfinexExchange(_configuration, _configuration.Credentials[clientXapiKey].ApiKey, _configuration.Credentials[clientXapiKey].ApiSecret, _log);
        }

        protected ExchangeBase GetUnAuthenticatedExchange()
        {
            return new BitfinexExchange(_configuration, String.Empty, String.Empty, _log);
        }

        
    }
}
