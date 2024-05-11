using DurableFunctionsApp.Net80;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading;

namespace DurableFunctionApp.Net80
{
    public static class SequenceExecutionFunction
    {
        [Function(nameof(SequenceExecutionFunction))]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] TaskOrchestrationContext context)
        {
            var outputs = new List<string>();

            outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), "Frankfurt"));
            outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), "Sarajevo"));
            outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), "Seattle"));

            // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
            return outputs;
        }

        [Function(nameof(SayHello))]
        public static string SayHello([ActivityTrigger] string name, FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger("SayHello");
            logger.LogInformation($"Saying hello to {name}.");
            return $"Hello {name}!";
        }


        [Function(nameof(HttpStartSequenceExecution))]
        public static async Task<IActionResult> HttpStartSequenceExecution(
         [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
         [DurableClient] DurableTaskClient client,
         FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger(nameof(HttpStartSequenceExecution));

            // Function input comes from the request content.
            string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
                nameof(SequenceExecutionFunction));

            logger.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

            try
            {
                // Returns an HTTP 202 response with an instance management payload.
                // See https://learn.microsoft.com/azure/azure-functions/durable/durable-functions-http-api#start-orchestration
                var res = client.GetAllInstancesAsync(new OrchestrationQuery() { InstanceIdPrefix = instanceId  });

                StringBuilder sb = new StringBuilder();

                var en = res.GetAsyncEnumerator();

                while(await en.MoveNextAsync()) 
                { 
                    sb.AppendLine($"{en.Current.Name}");
                    sb.AppendLine($"{en.Current.ToString()}");
                }

                return new OkObjectResult(sb);
                //return new MyResponse(executionContext, sb.ToString(), HttpStatusCode.OK);
                //var x = client.CreateCheckStatusResponse(req, instanceId);
            }
            catch (Exception ex)
            {
                throw;
            }          
        }
    }

    public class MyHttpCookies : HttpCookies
    {
        public override void Append(string name, string value)
        {
           
        }

        public override void Append(IHttpCookie cookie)
        {
            
        }

        public override IHttpCookie CreateNew()
        {
            return null;
        }
    }

    public class MyResponse : HttpResponseData
    {
        private Stream _stream;

        private HttpStatusCode _code;

        public MyResponse(FunctionContext functionContext, string content, HttpStatusCode code) : base(functionContext)
        {
            _code = code;
            _stream = new MemoryStream();
            MemoryStream memoryStream = new MemoryStream();
            StreamWriter streamWriter = new StreamWriter(memoryStream);
            streamWriter.Write(content);
            streamWriter.Flush();
            _stream.Flush();
            _stream.Position = 0;
        }

        public override HttpStatusCode StatusCode { get => _code; set => _code = value; }
        public override HttpHeadersCollection Headers { get => new HttpHeadersCollection(); set => throw new NotImplementedException(); }
        public override Stream Body { get => _stream; set => _stream = value; }

        public override HttpCookies Cookies { get => new MyHttpCookies(); }
    }
}