using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace DurableFunctionApp
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var tasks = new Task<string>[3];

            var outputs = new List<string>();

            // Replace "hello" with the name of your Durable Activity Function.
            tasks[0] = context.CallActivityAsync<string>("Task1", "Frankfurt");
            tasks[1] = context.CallActivityAsync<string>("Task2", "Seattle");
            tasks[2] = context.CallActivityAsync<string>("Task3", "Sarajevo");

            await Task.WhenAll(tasks);

            outputs.Add(tasks[0].Result);
            outputs.Add(tasks[1].Result);
            outputs.Add(tasks[2].Result);

            return outputs;
        }

        [FunctionName("Task1")]
        public static string Task1([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation($"Saying hello to {name}.");

            return $"1 {name}!";
        }

        [FunctionName("Task2")]
        public static string Task2([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation($"Saying hello to {name}.");

            return $"2 {name}!";
        }

        [FunctionName("Task3")]
        public static string Task3([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation($"Saying hello to {name}.");

            return $"3 {name}!";
        }

        [FunctionName("Function1_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("Function1", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}