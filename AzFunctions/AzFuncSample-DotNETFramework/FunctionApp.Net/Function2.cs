using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace FunctionApp.Net
{
    public static class Function2
    {
        [FunctionName("QueueTrigger")]
        public static void Run2([QueueTrigger("myqueue-items", Connection = "QueueConnStr")]string myQueueItem, TraceWriter log)
        {
            log.Info($"C# Queue trigger function processed: {myQueueItem}");
        }
    }
}
