using Azure.Messaging.EventHubs.Consumer;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

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

            var connectionString = "Endpoint=sb://azuretrainingeventhub.servicebus.windows.net/;SharedAccessKeyName=demo;SharedAccessKey=6zNGkIqk+Yt0EP/Z/56RSY+SJUcGleMP3dG2tXUH+8M=";
            connectionString = "Endpoint=sb://iothub-ns-iothub-773-16330244-14ffc413e7.servicebus.windows.net/;SharedAccessKeyName=iothubowner;SharedAccessKey=qkDEqA95JQHlIraadB9cgbYFSJLE+cpbS3b3fTAd92Q=";
            connectionString = "Endpoint=sb://iothub-ns-iotsvc-hub-2087823-b351dbdc9a.servicebus.windows.net/;SharedAccessKeyName=iothubowner;SharedAccessKey=uklkmLNgLPuMk2crINGsmavJ+c3kx7d7LpsEW2mE8cg=";
            connectionString = "Endpoint=sb://sensolus.servicebus.windows.net/;SharedAccessKeyName=listen;SharedAccessKey=Ro7IFg9fqvD+gLAYUnefPeSn2UZTwSRpRwhzVV5/F00=";

            var eventHubName = "myhub";
            eventHubName = "iothub-77336";
            eventHubName = "iotsvc-hub-dev";
            eventHubName = "webhook";

            //var token = CreateToken("https://sensolus.servicebus.windows.net", "send", "M62Xf6yZAf6EsmH6Mu2d6c48FcdmpDjJWTo4/lvDTPg=");

            var consumerGroup = EventHubConsumerClient.DefaultConsumerGroupName;

            var consumer = new EventHubConsumerClient(
                consumerGroup,
                connectionString,
                eventHubName);

            try
            {
                using CancellationTokenSource cancellationSource = new CancellationTokenSource();

                cancellationSource.CancelAfter(TimeSpan.FromDays(1));

                DateTimeOffset timeBoomark = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromSeconds(111));
                EventPosition startingPosition = EventPosition.FromEnqueuedTime(timeBoomark);

                var res = await consumer.GetPartitionIdsAsync();
                
                var partIds = await consumer.GetPartitionIdsAsync(cancellationSource.Token);

                //string firstPartition = (partIds).First();

                while (true)
                {
                    //foreach (var partId in partIds)
                    {
                        //await foreach (PartitionEvent partitionEvent in consumer.ReadEventsAsync(startReadingAtEarliestEvent: true, cancellationToken: cancellationSource.Token))

                        await foreach (PartitionEvent partitionEvent in consumer.ReadEventsAsync(new ReadEventOptions { PrefetchCount = 5 },
                        //await foreach (PartitionEvent partitionEvent in consumer.ReadEventsFromPartitionAsync(
                        //partId, startingPosition, new ReadEventOptions { PrefetchCount = 500 },

                        cancellationSource.Token))
                        {
                          //  if (partitionEvent.Data.SystemProperties["iothub-connection-device-id"] as string == "sensolus" && partitionEvent.Data.EnqueuedTime >= new DateTime(2022,10,18))
                        {
                                string readFromPartition = partitionEvent.Partition.PartitionId;

                                byte[] payload = partitionEvent.Data.EventBody.ToArray();

                                Console.WriteLine($"DeviceId: {partitionEvent.Data.SystemProperties["iothub-connection-device-id"]} - {partitionEvent.Data.EnqueuedTime}");
                                Console.WriteLine($"DeviceId: {partitionEvent.Data.SystemProperties["iothub-connection-device-id"]} - Read event of length {payload.Length} from {readFromPartition}. Event: {UTF8Encoding.UTF8.GetString(payload)}");
                              
                                SaveToFile(UTF8Encoding.UTF8.GetString(payload));
                                //Console.WriteLine($"Read event of length {payload.Length} from {readFromPartition}. ");
                            }
                        }
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


        /// <summary>
        /// https://NAMESPACENAME.servicebus.windows.net
        /// </summary>
        /// <param name="resourceUri"></param>
        /// <param name="keyName"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private static string CreateToken(string resourceUri, string keyName, string key)
        {
            TimeSpan sinceEpoch = DateTime.UtcNow - new DateTime(1970, 1, 1);
            var week = 60 * 60 * 24 * 7;
            var expiry = Convert.ToString((int)sinceEpoch.TotalSeconds + TimeSpan.FromDays(365*10).TotalSeconds);
            string stringToSign = HttpUtility.UrlEncode(resourceUri) + "\n" + expiry;
            HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign)));
            var sasToken = String.Format(CultureInfo.InvariantCulture, "SharedAccessSignature sr={0}&sig={1}&se={2}&skn={3}", HttpUtility.UrlEncode(resourceUri), HttpUtility.UrlEncode(signature), expiry, keyName);
            return sasToken;
        }

        private static void SaveToFile(string payload)
        {
            //JsonSerializer.Deserialize<SigFoxEvent>(payload);

            using (StreamWriter sw = new StreamWriter($"{DateTime.UtcNow.Ticks}.json"))
            {
                sw.Write(payload);
            }
        }
    }
}
