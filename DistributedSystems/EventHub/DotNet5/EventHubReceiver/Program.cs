using Azure.Messaging.EventHubs.Consumer;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EventHubReceiver
{
    /// <summary>
    /// https://github.com/Azure/azure-sdk-for-net/blob/master/sdk/eventhub/Azure.Messaging.EventHubs/samples/Sample05_ReadingEvents.md
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello EventHub Receiver!");

            var connectionString = "Endpoint=sb://azuretrainingeventhub.servicebus.windows.net/;SharedAccessKeyName=pubsub;SharedAccessKey=+HD6ooWBSaXO/eGo0N8J5HYcii0OZCBFQ+UywYZdzsk=;EntityPath=myhub";
            var eventHubName = "myhub";
            var consumerGroup = EventHubConsumerClient.DefaultConsumerGroupName;

            var consumer = new EventHubConsumerClient(
                consumerGroup,
                connectionString,
                eventHubName);

            try
            {
                using CancellationTokenSource cancellationSource = new CancellationTokenSource();

                cancellationSource.CancelAfter(TimeSpan.FromSeconds(3000));

                DateTimeOffset timeBoomark = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromHours(1));
                EventPosition startingPosition = EventPosition.FromEnqueuedTime(timeBoomark);

                var res = await consumer.GetPartitionIdsAsync();
                string firstPartition = (await consumer.GetPartitionIdsAsync(cancellationSource.Token)).First();

                while (true)
                {
                    //await foreach (PartitionEvent partitionEvent in consumer.ReadEventsAsync(startReadingAtEarliestEvent: true, cancellationToken: cancellationSource.Token))

                    await foreach (PartitionEvent partitionEvent in consumer.ReadEventsFromPartitionAsync(
                    firstPartition,
                    startingPosition,
                    cancellationSource.Token))
                    {
                        string readFromPartition = partitionEvent.Partition.PartitionId;

                        byte[] payload = partitionEvent.Data.EventBody.ToArray();
                        
                        Console.WriteLine($"Read event of length { payload.Length } from { readFromPartition }. Event: {UTF8Encoding.UTF8.GetString(payload)}");
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // This is expected if the cancellation token is
                // signaled.
            }
            finally
            {
                await consumer.CloseAsync();
            }

        }
    }
}
