using DtfSample;
using DtfSample.Player;
using DurableTask.Core;
using DurableTask.ServiceBus;
using DurableTask.ServiceBus.Tracking;
using System;
using System.Threading;

namespace DtfService.NetCore
{
    class Program
    {
        private static string m_ConnStr = "Endpoint=sb://students.servicebus.windows.net/;SharedAccessKeyName=dtf;SharedAccessKey=S1TYy8NRwRxaB8MpAb2hibG7dOY+YBpV/wquvcAEBfg=";
        private static string m_StorageConnStr = "DefaultEndpointsProtocol=https;AccountName=azfunctionsamples;AccountKey=NEjFcvFNL/G7Ugq9RSW59+PonNgql/yLq8qfaVZPhanV9aJUnQi2b6Oy3csvPZPGVJreD+RgVUJJFFTZdUBhAA==;EndpointSuffix=core.windows.net";

        private static TaskHubWorker m_TaskHubWorker;

        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("DTF host started...");

            string hubName = "DtfSampleHub2";
            IOrchestrationServiceInstanceStore instanceStore = new AzureTableInstanceStore(hubName, m_StorageConnStr);

            var orchestrationServiceAndClient = new ServiceBusOrchestrationService(m_ConnStr, hubName, instanceStore, null, null);

            orchestrationServiceAndClient.CreateIfNotExistsAsync().Wait();

            m_TaskHubWorker = new TaskHubWorker(orchestrationServiceAndClient);
          
            m_TaskHubWorker.AddTaskOrchestrations(typeof(TimerOrchestration));
            m_TaskHubWorker.AddTaskOrchestrations(typeof(HelloOrchestration));
            m_TaskHubWorker.AddTaskOrchestrations(typeof(MediaOrchestration));
            
            m_TaskHubWorker.AddTaskActivities(typeof(SchedulerTask), typeof(UserInputTask), typeof(PlayFileTask));

            
            m_TaskHubWorker.StartAsync().Wait();

            Thread.Sleep(int.MaxValue);
        }

        private static void runCronOrchestration()
        {

        }
    }
}
