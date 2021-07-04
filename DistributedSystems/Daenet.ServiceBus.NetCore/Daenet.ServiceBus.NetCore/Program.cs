using System;

namespace Daenet.ServiceBus.NetCore
{

    class Program
    {
     

        static void Main(string[] args)
        {
            //var data = QueueSamples.createMessage();

            //qName="distsys/myname/whateveryoulike"
            Console.WriteLine("=============================================================================================");
            Console.WriteLine("daenet GmbH, Frankfurt University of Applied Sciences - Cloud Computing & Distributed Systems");
            Console.WriteLine("=============================================================================================");

            //SbManagementSamples.PrepareQueue("queuesamples/sendreceive", false).Wait();
            //QueueSamples.RunAsync(10).Wait();

            //SbManagementSamples.PrepareQueue("queuesamples/sendreceive", true).Wait();
            //QueueSessionSamples.RunAsync(10).Wait();

            //DeadLetterMessagingSamples.RunAsync(10).Wait();

            //QueueReliableMessagingSamples.RunAsync(10).Wait();

            SbManagementSamples.PrepareTopic("topicsamples/sendreceive", false).Wait();
            TopicSample.RunAsync(100).Wait();

            //SbManagementSamples.PrepareTopic("topicsamples/sendreceive", true).Wait();
            //TopicSessionSample.RunAsync(10).Wait();

        }


    }
}
