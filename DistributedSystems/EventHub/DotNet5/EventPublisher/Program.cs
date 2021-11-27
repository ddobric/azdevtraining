using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventPublisher
{
    /// <summary>
    /// https://github.com/Azure/azure-sdk-for-net/blob/master/sdk/eventhub/Azure.Messaging.EventHubs/samples/Sample04_PublishingEvents.md
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello EventHub publisher");

            var connectionString = "Endpoint=sb://azuretrainingeventhub.servicebus.windows.net/;SharedAccessKeyName=pubsub;SharedAccessKey=+HD6ooWBSaXO/eGo0N8J5HYcii0OZCBFQ+UywYZdzsk=;EntityPath=myhub";
            var eventHubName = "myhub";

            var producer = new EventHubProducerClient(connectionString, eventHubName);

            try
            {
                string firstPartition = (await producer.GetPartitionIdsAsync()).First();

                var batchOptions = new CreateBatchOptions
                {
                    //PartitionId = firstPartition,       
                    PartitionKey = "DEVICE"
                };


                while (true)
                {
                    using (var eventBatch = await producer.CreateBatchAsync(batchOptions))
                    {
                        for (var index = 0; index < 500; ++index)
                        {
                            var serData = JsonConvert.SerializeObject(new
                            {
                                dateTime = DateTime.Now,
                                minForce = 123.45,
                                maxForce = 1E25,
                                currentForce = 111.32
                            });

                            var eventBody = new BinaryData(UTF8Encoding.UTF8.GetBytes(serData));

                            var eventData = new EventData(eventBody);

                            //
                            // Add some custome metadata.
                            eventData.Properties.Add("EventType", "daenet.com/azuretraining");
                            eventData.Properties.Add("priority", 1);
                            eventData.Properties.Add("counter", index);

                            if (!eventBatch.TryAdd(eventData))
                            {
                                throw new Exception($"The event at { index } could not be added.");
                            }
                        }

                        await producer.SendAsync(eventBatch);

                    }

                    await Task.Delay(5000);
                }
            }
            finally
            {
                await producer.CloseAsync();
            }
        }
    }
}
