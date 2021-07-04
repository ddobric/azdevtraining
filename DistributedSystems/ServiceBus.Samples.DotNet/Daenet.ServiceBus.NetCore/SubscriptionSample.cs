using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Daenet.ServiceBus.NetCore
{
    internal class SubscriptionSample
    {
        const string m_SendConnStr = "Endpoint=sb://sageintegration.servicebus.windows.net/;SharedAccessKeyName=KannAlles;SharedAccessKey=ms9YOjkKHTuFHAuM85UpjGnxHEpJj0BTK0gJENuv/Rs=";
        const string m_ReceiveConnStr = "Endpoint=sb://sageintegration.servicebus.windows.net/;SharedAccessKeyName=KannAlles;SharedAccessKey=ms9YOjkKHTuFHAuM85UpjGnxHEpJj0BTK0gJENuv/Rs=";

        const string m_TopicName = "d01/t-webshop";

        static ITopicClient topicClient;

        const string m_SubscriptionName = "respSub";//"sys2";
        static ISubscriptionClient subscriptionClient;

        static ISessionClient sessionClient;
        static string sessionId = "SESS01";


        public static async Task RunAsync()
        {
            const int numberOfMessages = 10;
            topicClient = new TopicClient(m_SendConnStr, m_TopicName);
            subscriptionClient = new SubscriptionClient(m_ReceiveConnStr, m_TopicName, m_SubscriptionName, receiveMode:ReceiveMode.PeekLock);
            sessionClient = new SessionClient(m_ReceiveConnStr, "testsessions", receiveMode: ReceiveMode.PeekLock);
            //await SendMessagesAsync(numberOfMessages, sessionId);
          
            //RegisterOnMessageHandlerAndReceiveMessages();
            await RegisterOnSessionMessageHandlerAndReceiveMessages();

            Console.ReadKey();

            await topicClient.CloseAsync();
        }

        static async Task SendMessagesAsync(int numberOfMessagesToSend, string sessId = null)
        {
            try
            {
                for (var i = 0; i < numberOfMessagesToSend; i++)
                {
                    // Create a new message to send to the queue.
                    string messageBody = $"Message {i}";
                    var message = new Message(Encoding.UTF8.GetBytes(messageBody));
                    message.UserProperties.Add("USECASE" ,"UC-2.1-REQ");
                    message.TimeToLive = TimeSpan.FromMinutes(1);
                    if (sessionId != null)
                        message.SessionId = sessionId;

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

        static void RegisterOnMessageHandlerAndReceiveMessages()
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

            // Register the function that processes messages.
            subscriptionClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
        }


        static async Task RegisterOnSessionMessageHandlerAndReceiveMessages()
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

            // Register the function that processes messages.
            var session = await sessionClient.AcceptMessageSessionAsync(sessionId);

            subscriptionClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);

        }

        static async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            // Process the message.
            Console.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");

            // Complete the message so that it is not received again.
            // This can be done only if the subscriptionClient is created in ReceiveMode.PeekLock mode (which is the default).
            await subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);

            // Note: Use the cancellationToken passed as necessary to determine if the subscriptionClient has already been closed.
            // If subscriptionClient has already been closed, you can choose to not call CompleteAsync() or AbandonAsync() etc.
            // to avoid unnecessary exceptions.
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
