using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ServiceBus.Messaging;
using System.IO;
using System.Threading.Tasks;

namespace FunctionApp.Net
{
    public static class Function3
    {
        [FunctionName("ServiceBusTrigger")]
        [return: ServiceBus("queueOut", AccessRights.Send, Connection = "SbConnStr")]
        public static BrokeredMessage Run(
            [ServiceBusTrigger("queueIn", AccessRights.Listen, Connection = "SbConnStr")]BrokeredMessage inMsg, 
            TraceWriter log)
        {
            log.Info($"C# ServiceBus queue trigger function processed message: {inMsg.MessageId}");

            Stream stream = inMsg.GetBody<Stream>();
            StreamReader reader = new StreamReader(stream);
            string newBody = reader.ReadToEnd();

            BrokeredMessage outMsg = new BrokeredMessage(newBody);

            return outMsg;
        }
    }
}
