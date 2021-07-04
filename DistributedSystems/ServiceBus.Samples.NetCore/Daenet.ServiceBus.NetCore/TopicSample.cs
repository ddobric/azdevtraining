using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Daenet.ServiceBus.NetCore
{
    internal class TopicSample
    {

        const string cTopicName = "topicsamples/sendreceive";

        public const string cSubscriptionName1 = "Subscription1";
        public const string cSubscriptionName2 = "Subscription2";

        static ITopicClient topicClient;

        static ISubscriptionClient subscriptionClient1;

        static ISubscriptionClient subscriptionClient2;


        public static async Task RunAsync(int numberOfMessages)
        {
            topicClient = new TopicClient(Credentials.Current.ConnStr, cTopicName);

            subscriptionClient1 = new SubscriptionClient(Credentials.Current.ConnStr, cTopicName, cSubscriptionName1, receiveMode: ReceiveMode.PeekLock);

            subscriptionClient2 = new SubscriptionClient(Credentials.Current.ConnStr, cTopicName, cSubscriptionName2, receiveMode: ReceiveMode.PeekLock);

            await SendMessagesAsync(numberOfMessages);

            Console.WriteLine("Press any key to start receiver...");
            Console.ReadKey();
            Console.ForegroundColor = ConsoleColor.Yellow;

            RegisterMessageHandlerAndReceiveMessages();

            Console.ReadKey();

            await topicClient.CloseAsync();
        }

        static async Task SendMessagesAsync(int numberOfMessagesToSend)
        {
            try
            {
                Random rnd = new Random();

                for (var i = 0; i < numberOfMessagesToSend; i++)
                {
                    var num = rnd.Next();

                    // Create a new message to send to the queue.
                    string messageBody = $"{i} - Random number {num}";

                    var message = new Message(Encoding.UTF8.GetBytes(messageBody));

                    // We set here some random number.
                    // In this sample, subscription filter is used
                    message.UserProperties.Add("Number", num);

                    message.TimeToLive = TimeSpan.FromMinutes(5);

                    // Write the body of the message to the console.
                    Console.WriteLine($"Sending message: {messageBody}");

                    // Send the message to the queue.
                    await topicClient.SendAsync(message);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"{DateTime.Now} :: Exception: {exception.Message}");
            }
        }


        static void RegisterMessageHandlerAndReceiveMessages()
        {
            // Configure the message handler options in terms of exception handling, number of concurrent messages to deliver, etc.
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                // Maximum number of concurrent calls to the callback ProcessMessagesAsync(), set to 1 for simplicity.
                // Set it according to how many messages the application wants to process in parallel.
                MaxConcurrentCalls = 1,

                // Indicates whether the message pump should automatically complete the messages after returning from user callback.
                // False below indicates the complete operation is handled by the user callback as in ProcessMessagesAsync().
                AutoComplete = false
            };

            subscriptionClient1.RegisterMessageHandler(ProcessMessagesAsync1, messageHandlerOptions);

            subscriptionClient2.RegisterMessageHandler(ProcessMessagesAsync2, messageHandlerOptions);
        }

        static async Task ProcessMessagesAsync1(Message message, CancellationToken token)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");

            await subscriptionClient1.CompleteAsync(message.SystemProperties.LockToken);
        }

        static async Task ProcessMessagesAsync2(Message message, CancellationToken token)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");

            await subscriptionClient2.CompleteAsync(message.SystemProperties.LockToken);
        }


        // Use this handler to examine the exceptions received on the message pump.
        static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            Console.WriteLine("Exception context for troubleshooting:");
            Console.WriteLine($"- Endpoint: {context.Endpoint}");
            Console.WriteLine($"- Entity Path: {context.EntityPath}");
            Console.WriteLine($"- Executing Action: {context.Action}");
            return Task.CompletedTask;
        }

    }
}
