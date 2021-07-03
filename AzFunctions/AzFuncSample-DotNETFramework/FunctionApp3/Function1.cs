
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;

using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace FunctionApp3
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequest req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            return name != null
                ? (ActionResult)new OkObjectResult($"Hello, {name}")
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }


        //[FunctionName("QueueTriggerSample")]
        //[return: Table("persons", Connection = "STORAGE_SETTING")]
        //public static Person Run(
        //        [QueueTrigger("myqueue-items", Connection = "STORAGE_SETTING")]JObject order,
        //        TraceWriter log)
        //{
        //    return new Person()
        //    {
        //        PartitionKey = "Orders",
        //        RowKey = Guid.NewGuid().ToString(),
        //        Name = order["Name"].ToString(),
        //        MobileNumber = order["MobileNumber"].ToString()
        //    };
        //}

        public class Person
        {
            public string PartitionKey { get; set; }
            public string RowKey { get; set; }
            public string Name { get; set; }
            public string MobileNumber { get; set; }
        }

        //[FunctionName("SbSample")]
        //public static async Task RunSb(
        //                        [ServiceBusTrigger("[QueueName]", Connection = "SbConnStr")]
        //                        Message myQueueItem,
        //                        Int32 deliveryCount,
        //                        DateTime enqueuedTimeUtc,
        //                        string messageId,
        //                        TraceWriter log)
        //{
        //    log.Info("C# SB trigger function processed a request.");

        //    log.Info($"C# ServiceBus queue trigger function processed message delivery count: {myQueueItem.SystemProperties.DeliveryCount}");

        //    QueueClient queueClient = new QueueClient("[SbConnStr]", "[QueueName]");

        //    ////await queueClient.DeadLetterAsync(myQueueItem.SystemProperties.LockToken);
        //    await queueClient.AbandonAsync(myQueueItem.SystemProperties.LockToken);
        //}
    }
}
