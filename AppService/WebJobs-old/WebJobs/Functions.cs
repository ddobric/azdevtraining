using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ServiceBus.Messaging;

namespace WebJobs
{
    public class Functions
    {
        // This function will get triggered/executed when a new message is written 
        // on an Azure Queue called queue.
        public static void ProcessQueueMessage([QueueTrigger("queue")] string message, TextWriter log)
        {
            log.WriteLine(message);
        }


        public static void RunQueueReader([QueueTrigger("myqueue-items", Connection = "QueueConnStr")]string myQueueItem,
            TraceWriter log)
        {
            log.Info($"C# Queue trigger function processed: {myQueueItem}");
        }


        public static void RunSbQueueReader(
           [ServiceBusTrigger("queueIn", AccessRights.Listen, Connection = "SbConnStr")]BrokeredMessage inMsg,
           [ServiceBus("queueOut", AccessRights.Send, Connection = "SbConnStr")]out BrokeredMessage outMsg,
           TraceWriter log)
        {
            log.Info($"C# ServiceBus queue trigger v1 function processed message: {inMsg.MessageId}");

            Stream stream = inMsg.GetBody<Stream>();
            StreamReader reader = new StreamReader(stream);
            string newBody = reader.ReadToEnd();

            outMsg = new BrokeredMessage(newBody);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timer"></param>
        /// <param name="log"></param>
        /// <param name="logger"></param>
        public static void RunTimer([TimerTrigger("%CRONEEXPRESSION%")] TimerInfo timer, TextWriter log/*, ILogger logger*/)
        {
            log.Write("Timer Executed");
        }
    }
}
