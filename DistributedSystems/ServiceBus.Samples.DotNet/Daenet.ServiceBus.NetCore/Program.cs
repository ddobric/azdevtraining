using System;

namespace Daenet.ServiceBus.NetCore
{

    class Program
    {
     

        static void Main(string[] args)
        {
            //var data = QueueSamples.createMessage();

            Console.WriteLine("======================================================");
            Console.WriteLine("Running...");
            Console.WriteLine("======================================================");

            //QueueSessionSamples.RunAsync(10).Wait();
            //DeadLetterMessagingSamples.RunAsync(10).Wait();
            QueueSamples.RunAsync(10).Wait();
            //QueueReliableMessagingSamples.RunAsync(10).Wait();
            //TopicSessionSample.RunAsync(10).Wait();
            //TopicSample.RunAsync(100).Wait();

        }

     
    }
}
