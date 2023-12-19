using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using System.Diagnostics;

namespace JaegerOpenTelemetryDotnetExample.ServiceB.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PingController : ControllerBase
    {
        private readonly ILogger _logger;

        public PingController(ILogger<PingController> logger){
            _logger = logger;
        }
        [HttpGet]
        public IActionResult Get()
        {
            _logger.LogInformation("In Service B GET method");
            var infoFromContext = Baggage.Current.GetBaggage("ExampleItem");

            using var source = new ActivitySource("ExampleTracer");

            // A span
            using var activity = source.StartActivity("In Service B GET method");
            activity?.SetTag("InfoServiceBReceived", infoFromContext);
            _logger.LogInformation("In Service B GET method, InfoServiceBReceived: {0}", infoFromContext);
            return Ok();
        }
    }
}