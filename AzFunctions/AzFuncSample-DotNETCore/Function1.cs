using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Queue;

namespace AzFuncSamples
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static void Run([QueueTrigger("myqueue-items")]string myQueueItem,
            [Queue("azfnc-outqueue") ]CloudQueue cloudQueue, ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
            cloudQueue.AddMessageAsync(new CloudQueueMessage(DateTime.Now.ToString()));
        }
    }
}
