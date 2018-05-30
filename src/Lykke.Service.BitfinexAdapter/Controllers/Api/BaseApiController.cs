using Common.Log;
using Lykke.Service.BitfinexAdapter.Core.Domain;
using Lykke.Service.BitfinexAdapter.Core.Domain.Settings;
using Lykke.Service.BitfinexAdapter.Services.Exchange;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Authentication;
using Lykke.Service.BitfinexAdapter.AzureRepositories;

namespace Lykke.Service.BitfinexAdapter.Controllers.Api
{
    [Produces("application/json")]
    public abstract class BaseApiController : Controller
    {
        protected readonly BitfinexAdapterSettings _configuration;
        private readonly ILimitOrderRepository _limitOrderRepository;
        protected readonly ILog _log;
        protected int DefaultTimeOutSeconds = 30;

        public BaseApiController(
            BitfinexAdapterSettings configuration,
            ILimitOrderRepository limitOrderRepository,
            ILog log)
        {
            _configuration = configuration;
            _limitOrderRepository = limitOrderRepository;
            _log = log;
        }

        protected BitfinexExchange GetAuthenticatedExchange()
        {
            if (Request.Headers.TryGetValue(Constants.XApiKeyHeaderName, out var clientXapiKey))
            {
                var creds = _configuration.Credentials.FirstOrDefault(x =>
                    x.InternalApiKey.Equals(clientXapiKey, StringComparison.InvariantCultureIgnoreCase));

                if (creds != null)
                {
                    return new BitfinexExchange(
                        _configuration,
                        _limitOrderRepository,
                        clientXapiKey,
                        creds.ApiKey,
                        creds.ApiSecret, _log);
                }
            }

            throw new AuthenticationException(Constants.AuthenticationError);
        }

        protected ExchangeBase GetUnAuthenticatedExchange()
        {
            return new BitfinexExchange(
                _configuration,
                _limitOrderRepository,
                "<anonymous>",
                String.Empty,
                String.Empty, _log);
        }

        
    }
}
