using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DeviceMessageWriter
{
    class Program
    {
        static string m_SbConnStr;
        static string m_EventHubPath;
        static string m_BlobStorageConnectionString;
        static string m_BlobStorageContainerName;
        static long m_BatchSize;
        static EventHubClient eventHubClient;

        static void Main(string[] args)
        {
            m_SbConnStr = ConfigurationManager.AppSettings["IotHub.ServiceConnStr"];
            m_EventHubPath = ConfigurationManager.AppSettings["IotHub.EventHubPath"];
            m_BlobStorageConnectionString = ConfigurationManager.AppSettings["BlobStorage.ConnectionString"];
            m_BlobStorageContainerName = ConfigurationManager.AppSettings["BlobStorage.ContainerName"];

            eventHubClient = EventHubClient.CreateFromConnectionString(m_SbConnStr, m_EventHubPath);

            EventData data = new EventData();

            var serData = JsonConvert.SerializeObject(new
            {
                dateTime = DateTime.Now,
                minForce = 123.45,
                maxForce = 345.21,
                currentForce = 111.32
            });

            Task.Run(() =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    eventHubClient.Send(new EventData(UTF8Encoding.UTF8.GetBytes(serData)));
                }
            });


            Console.WriteLine("Start receiving of messages\n");

            List<EventHubClient> clients = new List<EventHubClient>();

            for (int i = 0; i < 5; i++)
            {
                clients.Add(EventHubClient.CreateFromConnectionString(m_SbConnStr, m_EventHubPath));
            }
        
            foreach (var client in clients)
            {
                var inf = client.GetRuntimeInformation();
                var partitions = client.GetRuntimeInformation().PartitionIds;

                foreach (string partition in partitions)
                    receiveMessagesFromDeviceAsync(partition);
            }

            Console.ReadLine();
        }

        private async static Task receiveMessagesFromDeviceAsync(string partition)
        {
            var grp = eventHubClient.GetConsumerGroup("test");
            var eventHubReceiver = grp.CreateReceiver(partition, DateTime.Now);

            long counter = 0;

            StringBuilder csvContent = new StringBuilder();
            while (true)
            {
                EventData eventData = await eventHubReceiver.ReceiveAsync();

                if (eventData == null)
                    continue;

                string data = Encoding.UTF8.GetString(eventData.GetBytes());

                csvContent.AppendLine(deserializeMessage(data));
                counter++;
                Console.WriteLine(string.Format("Message received. Partition: {0} Data: '{1}'", partition, data));

                if (m_BatchSize == counter)
                {
                    counter = 0;
                    csvContent.Clear();
                }

            }
        }

        private static string deserializeMessage(string data)
        {
            dynamic obj = JsonConvert.DeserializeObject(data);
            // 07.03.16 16:33:34,PI2-01,+02000,+00025,-02000
            string res = $"{obj.dateTime.ToString("dd.MM.yy HH:mm:ss")},{obj.deviceId},{obj.minForce},{obj.currentForce},{obj.maxForce}";

            return res;
        }


        private static void writeCsvToBlobStorage(StringBuilder csvContent, string containerName, string fileName)
        {

        }





    }
}
