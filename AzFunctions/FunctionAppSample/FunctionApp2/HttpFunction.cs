using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FunctionApp2
{
    public class HttpFunction
    {
        private readonly ILogger<HttpFunction> _logger;

        public HttpFunction(ILogger<HttpFunction> logger)
        {
            _logger = logger;
        }

        [Function("HttpFunction")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            return new OkObjectResult("Welcome to Azure Functions!");
        }
    }
}
