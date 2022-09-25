using Azure.Messaging.ServiceBus;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Daenet.ServiceBus.NetCore
{
    internal class TopicSample
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

            await SendMessagesAsync(numberOfMessages, topicName);

            Console.WriteLine("Press any key to start receiver...");

            Console.ReadKey();

            Console.ForegroundColor = ConsoleColor.Yellow;

            List<Task> tasks = new List<Task>();

            tasks.Add(RunSubscriptionReceiver(topicName, sub1, tokenSource.Token, ConsoleColor.Yellow));
            tasks.Add(RunSubscriptionReceiver(topicName, sub2, tokenSource.Token, ConsoleColor.Cyan));

            Task.WaitAll(tasks.ToArray());

            Console.ReadKey();
        }

        static async Task SendMessagesAsync(int numberOfMessagesToSend, string topicName)
        {
            try
            {
                var sender = m_SbClient.CreateSender(topicName);

                Random rnd = new Random();

                for (var i = 0; i < numberOfMessagesToSend; i++)
                {
                    var num = rnd.Next();

                    // Create a new message to send to the queue.
                    string messageBody = $"{i} - Random number {num}";

                    var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(messageBody));

                    // We set here some random number.
                    // In this sample, subscription filter is used
                    message.ApplicationProperties.Add("Number", num);

                    message.ApplicationProperties.Add("Code", 2);

                    message.TimeToLive = TimeSpan.FromMinutes(5);

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


        static async Task RunSubscriptionReceiver(string topicName, string subscriptionName, CancellationToken token, ConsoleColor color)
        {
            var receiver = m_SbClient.CreateReceiver(topicName, subscriptionName, new ServiceBusReceiverOptions { ReceiveMode = ServiceBusReceiveMode.PeekLock });

            while (token.IsCancellationRequested == false)
            {
                // Demonstrates how to receive a batch of messages.
                var msgs = await receiver.ReceiveMessagesAsync(maxMessages: 100, maxWaitTime: TimeSpan.FromSeconds(30));

                foreach (var message in msgs)
                {
                    Console.ForegroundColor = color;
                    Console.WriteLine($"Received message: {subscriptionName}, SequenceNumber:{message.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");

                    await receiver.CompleteMessageAsync(message);
                }
            }
        }
    }
}
