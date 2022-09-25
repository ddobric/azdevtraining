using System;
using System.Threading.Tasks;

namespace Daenet.ServiceBus.NetCore
{
    /// <summary>
    /// Samples that demonstrates most important Service Bus features.
    /// </summary>
    class Program
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        static async Task Main(string[] args)
        {
            Console.WriteLine("=============================================================================================");
            Console.WriteLine("daenet GmbH - ACP Digital");
            Console.WriteLine("=============================================================================================");
            
            const string queueName = "queuesamples/sendreceive";
            const string queueNameSession = "queuesamples/sendreceive-session";
            const string topicName = "topicsamples/sendreceive";
            const string topicNameSession = "topicsamples/sendreceive-session";

            //await SbManagementSamples.EnsureQueueExists(queueName, false);
            //await QueueSamples.RunAsync(500, queueName);

            //await SbManagementSamples.EnsureQueueExists(queueNameSession, true);
            //await QueueSessionSamples.RunAsync(10, queueNameSession);

            //await SbManagementSamples.EnsureQueueExists(queueName, false);
            //await DeadLetterMessagingSamples.RunAsync(10, queueName);

            //await SbManagementSamples.EnsureQueueExists(queueName, false);
            //await QueueReliableMessagingSamples.RunAsync(10, queueName);

            //await SbManagementSamples.EnsureTopicExists(topicName, "subscription1", "subscription2", false);
            //await TopicSample.RunAsync(100, topicName, "subscription1", "subscription2");

            await SbManagementSamples.EnsureTopicExists(topicNameSession, "session-subscription1", "session-subscription2", true);
            await TopicSessionSample.RunAsync(10, topicNameSession, "session-subscription1", "session-subscription2");
        }
    }
}
