using DtfSample;
using DtfSample.Player;
using DurableTask;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DtfService
{
    class Program
    {
        private static string m_ConnStr = "Endpoint=sb://bastasample.servicebus.windows.net/;SharedAccessKeyName=demo;SharedAccessKey=MvwVbrrJdsMQyhO/0uwaB5mVbuXyvYa3WRNpalHi0LQ=";
        private static string m_StorageConnStr = "DefaultEndpointsProtocol=https;AccountName=azfunctionsamples;AccountKey=NEjFcvFNL/G7Ugq9RSW59+PonNgql/yLq8qfaVZPhanV9aJUnQi2b6Oy3csvPZPGVJreD+RgVUJJFFTZdUBhAA==;EndpointSuffix=core.windows.net";

        private static TaskHubWorker m_TaskHubWorker;

        static void Main(string[] args)
        {
            m_StorageConnStr = null;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("DTF host started...");

            m_TaskHubWorker = new TaskHubWorker("DtfSampleHub2", m_ConnStr, m_StorageConnStr);
            m_TaskHubWorker.CreateHubIfNotExists();

            m_TaskHubWorker.AddTaskOrchestrations(typeof(TimerOrchestration));
            m_TaskHubWorker.AddTaskOrchestrations(typeof(HelloOrchestration));
            m_TaskHubWorker.AddTaskActivities(typeof(SchedulerTask), typeof(UserInputTask), typeof(PlayFileTask));

            m_TaskHubWorker.Start();
            
            Thread.Sleep(int.MaxValue);
        }

        private static void runCronOrchestration()
        {

        }
    }
}
