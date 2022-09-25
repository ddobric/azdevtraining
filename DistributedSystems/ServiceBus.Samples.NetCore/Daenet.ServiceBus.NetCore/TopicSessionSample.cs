
using Azure.Messaging.ServiceBus;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Daenet.ServiceBus.NetCore
{
    internal class TopicSessionSample
    {
        private static CancellationTokenSource tokenSource = new CancellationTokenSource();

        /// <summary>
        /// Client for sending and receiving queue messages.
        /// </summary>
        static ServiceBusClient m_SbClient;


        public static async Task RunAsync(int numberOfMessages, string topicName, string sub1, string sub2)
        {
            ServiceBusClientOptions opts = new ServiceBusClientOptions
            {
                RetryOptions = new ServiceBusRetryOptions { MaxRetries = 3, MaxDelay = TimeSpan.FromMinutes(3), Delay = TimeSpan.FromMinutes(1) }
            };

            m_SbClient = new ServiceBusClient(Credentials.Current.ConnStr, opts);

            await SendMessagesAsync(numberOfMessages, "S1", topicName);
            await SendMessagesAsync(numberOfMessages, "S2", topicName);
            await SendMessagesAsync(numberOfMessages, "S3", topicName);

            Console.ForegroundColor = ConsoleColor.Yellow;

            List<Task> tasks = new List<Task>();

            tasks.Add(RunSubscriptionSessionReceivers(topicName, sub1, tokenSource.Token));
            tasks.Add(RunSubscriptionSessionReceivers(topicName, sub2, tokenSource.Token));

            Task.WaitAll(tasks.ToArray());

            Console.WriteLine("Press any key to start receiver...");
            Console.ReadKey();
            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.ReadKey();
        }

        static async Task SendMessagesAsync(int numberOfMessagesToSend, string sessId, string queueName)
        {
            try
            {
                var sender = m_SbClient.CreateSender(queueName);

                for (var i = 0; i < numberOfMessagesToSend; i++)
                {
                    // Create a new message to send to the queue.
                    string messageBody = $"Message {i}";
                    var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(messageBody));
                    message.ApplicationProperties.Add("USECASE", "Topic Session Sample");
                    message.TimeToLive = TimeSpan.FromMinutes(10);
                    if (sessId != null)
                        message.SessionId = sessId;

                    // Write the body of the message to the console.
                    Console.WriteLine($"Sending message: {messageBody}");

                    // Send the message to the queue.
                    await sender.SendMessageAsync(message);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"{DateTime.Now} :: Exception: {exception.Message}");
            }
        }



        static Task RunSubscriptionSessionReceivers(string topicName, string subscriptionName, CancellationToken token)
        {
            return Task.Run(() =>
            {
                List<Task> tasks = new List<Task>();

                List<string> sessions = new List<string>();
                sessions.Add("S1");
                sessions.Add("S2");
                sessions.Add("S3");

                foreach (var sess in sessions)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        // create a session receiver that we can use to receive the message. Since we don't specify a
                        // particular session, we will get the next available session from the service.
                        ServiceBusSessionReceiver sessReceiver = await m_SbClient.AcceptSessionAsync(topicName, subscriptionName, sess);

                        while (!token.IsCancellationRequested)
                        {
                            var message = await sessReceiver.ReceiveMessageAsync();

                            if (message != null)
                            {
                                Console.WriteLine($"Received message {subscriptionName}: SessionId:{message.SessionId}, SequenceNumber:{message.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");
                                await sessReceiver.CompleteMessageAsync(message);
                            }
                            else
                            {
                                Console.WriteLine("no messages..");
                            }
                        }
                    }));
                }

                Task.WaitAll(tasks.ToArray());

            });
        }
    }
}
