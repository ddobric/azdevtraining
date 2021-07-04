using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Daenet.ServiceBus.NetCore
{
    /// <summary>
    /// Demonstrates how to send and receive messages from queue.
    /// This sample implements reliable (PeeckLock) pattern.
    /// </summary>
    internal class QueueReliableMessagingSamples
    {
        const string m_QueueName = "queuesamples/sendreceive";

        /// <summary>
        /// Client for sending and receiving queue messages.
        /// </summary>
        static IQueueClient m_QueueClient;

        /// <summary>
        /// Start sending of messages.
        /// </summary>
        /// <param name="numberOfMessages"></param>
        /// <param name="autoComplete">True if messaging is none reliable. Use false for reliable messaging.</param>
        /// <returns></returns>
        public static async Task RunAsync(int numberOfMessages)
        {
            m_QueueClient = new QueueClient(Credentials.Current.ConnStr,
                m_QueueName,
                receiveMode: ReceiveMode.PeekLock);

            if (numberOfMessages <= 100)
                await SendMessagesAsync(numberOfMessages);
            else
                await SendMessageBatchAsync(numberOfMessages);

            Console.WriteLine("Press any key to start receiver...");
            Console.ReadKey();

            Console.ForegroundColor = ConsoleColor.Yellow;

            RegisterOnMessageHandlerAndReceiveMessages();

            Console.ReadKey();

            await m_QueueClient.CloseAsync();
        }

        /// <summary>
        /// Send message by message to the queue.
        /// </summary>
        /// <param name="numberOfMessagesToSend"></param>
        /// <returns></returns>
        static async Task SendMessagesAsync(int numberOfMessagesToSend)
        {
            try
            {
                for (var i = 0; i < numberOfMessagesToSend; i++)
                {
                    var message = new Message(createMessage());

                    message.MessageId = Guid.NewGuid().ToString();

                    message.UserProperties.Add("USECASE", "Test Reliable Messaging");
                
                    Console.WriteLine($"Sending message: {message.MessageId}");

                    // Send the message to the queue.
                    await m_QueueClient.SendAsync(message);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"{DateTime.Now} : Exception: {exception.Message}");
            }
        }

        /// <summary>
        /// Send message by message to the queue.
        /// </summary>
        /// <param name="numberOfMessagesToSend"></param>
        /// <returns></returns>
        static async Task SendMessageBatchAsync(int numberOfMessagesToSend)
        {
            try
            {
                List<Message> messages = new List<Message>();

                for (var i = 0; i < numberOfMessagesToSend; i++)
                {
                    // Create a new message to send to the queue.
                    string messageBody = $"Message {i}";
                    var message = new Message(Encoding.UTF8.GetBytes(messageBody));
                    message.UserProperties.Add("USECASE", "UC-2.1-REQ");
                    message.TimeToLive = TimeSpan.FromMinutes(10);
                    Console.WriteLine($"Adding message to batch: {messageBody}");
                    messages.Add(message);
                }

                // Send batch of messages to the queue.
                await m_QueueClient.SendAsync(messages);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"{DateTime.Now} : Exception: {exception.Message}");
            }
        }


        /// <summary>
        /// Register two handlers: Message Receive- and Error-handler.
        /// </summary>
        static void RegisterOnMessageHandlerAndReceiveMessages()
        {
            // Configure the message handler options in terms of exception handling, number of concurrent messages to deliver, etc.
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                // Maximum number of concurrent calls to the callback ProcessMessagesAsync(), set to 1 for simplicity.
                // Set it according to how many messages the application wants to process in parallel.
                MaxConcurrentCalls = 5,

                // Indicates whether the message pump should automatically complete the messages after returning from user callback.
                // False below indicates the complete operation is handled by the user callback as in ProcessMessagesAsync().
                AutoComplete = false
            };

            // Register the function that processes messages with reliable messaging.
            m_QueueClient.RegisterMessageHandler(ProcessMessagesReliableAsync, messageHandlerOptions);
        }


        /// <summary>
        /// Demonstrates how to process message in a reliable way. If the complete is not called,
        /// message remains in a queue for LockTime duration.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        static async Task ProcessMessagesReliableAsync(Message message, CancellationToken token)
        {
            // Process the message.
            Console.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");

            var json = Encoding.UTF8.GetString(message.Body);

            Transfer transferObj = JsonConvert.DeserializeObject<Transfer>(json);
        
            if (transferObj.amount < 10000)
            {
                Console.WriteLine($"Ammount: {transferObj.amount}, From:{ transferObj.fromAccount}, To: {transferObj.toAccount}.");

                // Complete the message so that it is not received again.
                // This can be done only if the subscriptionClient is created in ReceiveMode.PeekLock mode (which is the default).
                await m_QueueClient.CompleteAsync(message.SystemProperties.LockToken);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Ammount: {transferObj.amount}, From:{ transferObj.fromAccount}, To: {transferObj.toAccount}.");
                await m_QueueClient.AbandonAsync(message.SystemProperties.LockToken);
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Invoked in a case of an error.
        /// </summary>
        /// <param name="exceptionReceivedEventArgs"></param>
        /// <returns></returns>
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

        private static Random m_Rnd = new Random();

        internal static byte[] createMessage()
        {
            var obj = new Transfer()
            {
                fromAccount = "DE123",
                toAccount = "DE456",
                amount = (Decimal)(m_Rnd.Next(20000) + m_Rnd.NextDouble()),
            };

            string jsonObj = JsonConvert.SerializeObject(obj);

            byte[] binData = Encoding.UTF8.GetBytes(jsonObj);

            return binData;
        }


    }
}
