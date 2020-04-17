using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AppConfigTest
{
    public class TestFunction
    {
        private readonly Test _config;
        private readonly IConfigurationRefresher _refresher;

        public TestFunction(
            IOptionsSnapshot<Test> config,
            IConfigurationRefresher refresher)
        {
            _refresher = refresher;
            _config = config.Value;
        }

        [FunctionName("TestFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            if (bool.TryParse(req.Query["refresh"], out var result) && result)
            {
                await _refresher.RefreshAsync();
            }

            var responseMessage = $"Message: {_config.Message}{Environment.NewLine}Secret: {_config.Secret}";
            return new OkObjectResult(responseMessage);
        }
    }
}