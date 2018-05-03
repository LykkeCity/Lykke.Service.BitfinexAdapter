using Lykke.Service.BitfinexAdapter.Core.Domain;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;

namespace Lykke.Service.BitfinexAdapter.Authentication
{
    public class SwaggerXApiHeader : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            var filterPipeline = context.ApiDescription.ActionDescriptor.FilterDescriptors;
            var requiresAuthentication = filterPipeline.Select(filterInfo => filterInfo.Filter).Any(filter => filter is ApiKeyAuthAttribute);
            if (requiresAuthentication)
            {
                if (operation.Parameters == null)
                    operation.Parameters = new List<IParameter>();

                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = Constants.XApiKeyHeaderName,
                    In = "header",
                    Description = "Client's Bitfinex access token",
                    Required = true,
                    Type = "string"
                });
            }
        }
    }
}
