using Common.Log;
using Lykke.Service.BitfinexAdapter.Core.Domain;
using Lykke.Service.BitfinexAdapter.Core.Domain.Settings;
using Lykke.Service.BitfinexAdapter.Logging;
using Lykke.Service.BitfinexAdapter.Services.Exchange;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System;
using System.Linq;
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
            if (Request.Headers.TryGetValue(Constants.XApiKeyHeaderName, out var clientXapiKey))
            {
                var creds = _configuration.Credentials.FirstOrDefault(x =>
                    x.InternalApiKey.Equals(clientXapiKey, StringComparison.InvariantCultureIgnoreCase));

                if (creds != null)
                {
                    return new BitfinexExchange(_configuration, creds.ApiKey, creds.ApiSecret, _log);
                }
            }

            throw new AuthenticationException(Constants.AuthenticationError);
        }

        protected ExchangeBase GetUnAuthenticatedExchange()
        {
            return new BitfinexExchange(_configuration, String.Empty, String.Empty, _log);
        }

        
    }
}
