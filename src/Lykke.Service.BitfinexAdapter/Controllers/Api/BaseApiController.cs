using Lykke.Service.BitfinexAdapter.Logging;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.BitfinexAdapter.Controllers.Api
{
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    [LoggingAspNetFilter]
    public abstract class BaseApiController : Controller
    {
       
    }
}
