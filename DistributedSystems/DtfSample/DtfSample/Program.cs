using DurableTask;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DtfSample
{
    class Program
    {
        private static string m_ConnStr = "Endpoint=sb://bastasample.servicebus.windows.net/;SharedAccessKeyName=demo;SharedAccessKey=MvwVbrrJdsMQyhO/0uwaB5mVbuXyvYa3WRNpalHi0LQ=";
        private static string m_StorageConnStr = "DefaultEndpointsProtocol=https;AccountName=azfunctionsamples;AccountKey=NEjFcvFNL/G7Ugq9RSW59+PonNgql/yLq8qfaVZPhanV9aJUnQi2b6Oy3csvPZPGVJreD+RgVUJJFFTZdUBhAA==;EndpointSuffix=core.windows.net";

        static TaskHubClient m_TaskHubClient;

        static void Main(string[] args)
        {
            m_TaskHubClient = new TaskHubClient("DtfSampleHub2", m_ConnStr, m_StorageConnStr);

            //var instanceId = runCronSample();
            var instanceId = runHelloSample();

            var query = new OrchestrationStateQuery();
            query.AddInstanceFilter(instanceId);

            while (true)
            {
                var states = m_TaskHubClient.QueryOrchestrationStates(query);
                foreach (var state in states)
                {
                    Thread.Sleep(10000);
                    Console.WriteLine(state.OrchestrationStatus);
                }
            }
        }


        private static string runHelloSample()
        {
            var instance = m_TaskHubClient.CreateOrchestrationInstance(typeof(HelloOrchestration), Guid.NewGuid().ToString(),  "some state :)");

            return instance.InstanceId;

        }

        private static string runCronSample()
        {
            var instance = m_TaskHubClient.CreateOrchestrationInstance(typeof(TimerOrchestration), Guid.NewGuid().ToString(), "some state :)");

            return instance.InstanceId;

        }
    }
}
