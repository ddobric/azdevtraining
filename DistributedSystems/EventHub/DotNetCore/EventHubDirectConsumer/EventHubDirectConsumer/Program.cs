using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Newtonsoft.Json;

namespace EventHubDirectConsumer
{
    class Program
    {
        static EventHubClient eventHubClient;

        static string m_SbConnStr = "Endpoint=sb://traininghub.servicebus.windows.net/;SharedAccessKeyName=policy-hub1;SharedAccessKey=QdBEwItDWY/CyPHlZIbTOTL346KMRsi9wYrHW+xSXMc=;EntityPath=hub1";

        static string m_ConsumerGroupName = "$Default";

        public static async Task Main(string[] args)
        {
            eventHubClient = EventHubClient.CreateFromConnectionString(m_SbConnStr);

            var serData = JsonConvert.SerializeObject(new
            {
                dateTime = DateTime.Now,
                minForce = 123.45,
                maxForce = 345.21,
                currentForce = 111.32
            });

            await Task.Run(async () =>
            {
                for (int i = 0; i < 100; i++)
                {
                    await eventHubClient.SendAsync(new EventData(UTF8Encoding.UTF8.GetBytes(serData)));
                }
            });


            Console.WriteLine("Start receiving of messages\n");

            List<EventHubClient> clients = new List<EventHubClient>();

            for (int i = 0; i < 5; i++)
            {
                clients.Add(EventHubClient.CreateFromConnectionString(m_SbConnStr));
            }

            foreach (var client in clients)
            {
                var inf = await client.GetRuntimeInformationAsync();
                var partitions = inf.PartitionIds;

                foreach (string partition in partitions)
                    receiveMessagesFromDeviceAsync(partition);
            }

            Console.ReadLine();
        }
        
        private async static Task receiveMessagesFromDeviceAsync(string partition)
        {
            var eventHubReceiver = eventHubClient.CreateReceiver(m_ConsumerGroupName, partition, EventPosition.FromStart());

            long counter = 0;

            StringBuilder csvContent = new StringBuilder();
            while (true)
            {
                var events = await eventHubReceiver.ReceiveAsync(100);

                if (events == null)
                    continue;

                foreach (var eventData in events)
                {
                    string data = Encoding.UTF8.GetString(eventData.Body);

                    csvContent.AppendLine(deserializeMessage(data));
                    counter++;
                    Console.WriteLine(string.Format("Message received. Partition: {0} Data: '{1}'", partition, data));

                    if (100 == counter)
                    {
                        counter = 0;
                        csvContent.Clear();
                    }
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
    }
}
