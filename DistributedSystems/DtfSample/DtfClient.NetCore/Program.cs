using DtfSample;
using DtfSample.Player;
using DurableTask.Core;
using DurableTask.ServiceBus;
using DurableTask.ServiceBus.Tracking;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.ContextImplementations;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;

namespace DtfClient.NetCore
{
    class Program
    {
        static string serviceBusConnectionString = "Endpoint=sb://students.servicebus.windows.net/;SharedAccessKeyName=dtf;SharedAccessKey=S1TYy8NRwRxaB8MpAb2hibG7dOY+YBpV/wquvcAEBfg=";
        static string storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=azfunctionsamples;AccountKey=NEjFcvFNL/G7Ugq9RSW59+PonNgql/yLq8qfaVZPhanV9aJUnQi2b6Oy3csvPZPGVJreD+RgVUJJFFTZdUBhAA==;EndpointSuffix=core.windows.net";
        static string taskHubName = "DtfSampleHub2";

        static void Main(string[] args)
        {
            Console.WriteLine("daenet Durable Task Framework!");

            IOrchestrationServiceInstanceStore instanceStore = new AzureTableInstanceStore(taskHubName, storageConnectionString);

            //Microsoft.Azure.WebJobs.Extensions.DurableTask.DurableClient c;
            var orchestrationServiceAndClient =
                new ServiceBusOrchestrationService(serviceBusConnectionString, taskHubName, instanceStore, null, null);

            var taskHubClient = new TaskHubClient(orchestrationServiceAndClient);
            
            //var instance = taskHubClient.CreateOrchestrationInstanceAsync(typeof(MediaOrchestration), new Random().Next().ToString(), null).Result;
            var instance = taskHubClient.CreateOrchestrationInstanceAsync(typeof(HelloOrchestration), new Random().Next().ToString(), null).Result;
            //var instance = taskHubClient.CreateOrchestrationInstanceAsync(typeof(TimerOrchestration), new Random().Next().ToString(), "Hello").Result;

            OrchestrationState taskResult = taskHubClient.WaitForOrchestrationAsync(instance, TimeSpan.FromSeconds(60), CancellationToken.None).Result;
            Console.WriteLine($"Task done: {taskResult?.OrchestrationStatus}");

            Console.WriteLine("Press any key to quit.");
            Console.ReadLine();

        }

        static void TalkToEntity(string[] args)
        {
            ////IOptions<DurableClientOptions> defaultDurableClientOptions =
            ////AzureStorageDurabilityProviderFactory
            // IDurableClientFactory durableClientFactory = new DurableClientFactory(new ClientOptions("", "cpdmfuncinterfacestest"), (o) => { }, 
            ////IDurableClientFactory clientFact = new DurableClientFactory()
                    }

        public class ClientOptions : IOptions<DurableClientOptions>
        {
            private DurableClientOptions options;
            public ClientOptions(string connName, string taskHubName)
            {
                this.options = new DurableClientOptions()
                {
                    ConnectionName = connName,
                    IsExternalClient = true,
                    TaskHub = taskHubName,
                };
            }

            public DurableClientOptions Value => new DurableClientOptions();
        }
    }
}
