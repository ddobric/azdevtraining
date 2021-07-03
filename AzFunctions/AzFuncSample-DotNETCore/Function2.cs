using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace AzFuncSamples
{
    public static class Function2
    {
        [FunctionName("Function2")]
        public static void Run([ServiceBusTrigger("myqueue", Connection = "MySbConnStr")]string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");
        }
    }
}
