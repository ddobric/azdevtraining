using DurableFunctionsApp.Net80;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;

namespace DurableFunctionApp
{
    public static class ParallelExecutionFunction
    {
        [Function("ParallelExecutionFunction")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] TaskOrchestrationContext context)
        {
            var tasks = new Task<string>[3];

            var outputs = new List<string>();

            // Replace "hello" with the name of your Durable Activity Function.
            tasks[0] = context.CallActivityAsync<string>("Task1", "Frankfurt");
            tasks[1] = context.CallActivityAsync<string>("Task2", "Seattle");
            tasks[2] = context.CallActivityAsync<string>("Task3", "Sarajevo");

            await Task.WhenAll(tasks);

            await context.CallActivityAsync<string>("AllDone", "completed");

            outputs.Add(tasks[0].Result);
            outputs.Add(tasks[1].Result);
            outputs.Add(tasks[2].Result);

            return outputs;
        }

        [Function("Task1")]
        public static string Task1([ActivityTrigger] string name, FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger("Task1");

            return $"1 {name}!";
        }

        [Function("Task2")]
        public static string Task2([ActivityTrigger] string name, FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger("Task2");

            logger.LogInformation($"Saying hello to {name}.");

            return $"2 {name}!";
        }

        [Function("Task3")]
        public static string Task3([ActivityTrigger] string name, FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger("Task2");

            return $"3 {name}!";
        }

        [Function("AllDone")]
        public static string AllDone([ActivityTrigger] string name, FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger("AllDone");

            return $"3 {name}!";
        }

        [Function(nameof(HttpStartParallelExecutionFunction))]
        public static async Task<HttpResponseMessage> HttpStartParallelExecutionFunction(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] DurableTaskClient client,
            FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger(nameof(HttpStartParallelExecutionFunction));

            // Function input comes from the request content.
            string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nameof(ParallelExecutionFunction), null);

            logger.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            var ret = new HttpResponseMessage(HttpStatusCode.BadGateway);

           ret.Content = new StringContent("text", Encoding.UTF8, "application/json");

            return ret;
            //return new MyResponse(executionContext, "sb.ToString()", HttpStatusCode.OK);
        }
    }
}