using Lykke.Service.BitfinexAdapter.Core.Domain;
using Lykke.Service.BitfinexAdapter.Core.Settings.ServiceSettings;
using Lykke.Service.BitfinexAdapter.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Net;

namespace Lykke.Service.BitfinexAdapter.Authentication
{
    public sealed class ApiKeyAuthAttribute : ActionFilterAttribute
    {
        internal static Dictionary<string, ApiKeyCredentials> ClientApiKeys { get; set; }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.HttpContext.Request.Headers.ContainsKey(Constants.XApiKeyHeaderName))
            {
                context.Result = new BadRequestObjectResult($"No {Constants.XApiKeyHeaderName} header");
            }
            else
            {
                var apiKeyFromRequest = context.HttpContext.Request.Headers[Constants.XApiKeyHeaderName];

                if (!ClientApiKeys.ContainsKey(apiKeyFromRequest) || String.IsNullOrWhiteSpace(ClientApiKeys[apiKeyFromRequest].ApiKey) || String.IsNullOrWhiteSpace(ClientApiKeys[apiKeyFromRequest].ApiSecret))
                {
                    context.Result = new ObjectResult(new ErrorModel(Constants.AuthenticationError, (int)HttpStatusCode.Unauthorized))
                    {
                        StatusCode = (int)HttpStatusCode.Unauthorized
                    }; 
                }
            }
            base.OnActionExecuting(context);
        }
    }
}
