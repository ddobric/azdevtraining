
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
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
    /// This sample implements reliable (PeeckLock) and non reliable (ReceiveDelete)
    /// patterns.
    /// </summary>
    internal class QueueSamples
    {
        private static CancellationTokenSource tokenSource = new CancellationTokenSource();

        /// <summary>
        /// Client for sending and receiving queue messages.
        /// </summary>
        static ServiceBusClient m_SbClient;

        /// <summary>
        /// Start sending of messages.
        /// </summary>
        /// <param name="numberOfMessages"></param>
        /// <param name="autoComplete">True if messaging is none reliable. Use false for reliable messaging.</param>
        /// <returns></returns>
        public static async Task RunAsync(int numberOfMessages, string queueName)
        {
            ServiceBusClientOptions opts = new ServiceBusClientOptions
            {
                RetryOptions = new ServiceBusRetryOptions { MaxRetries = 3, MaxDelay = TimeSpan.FromMinutes(3), Delay = TimeSpan.FromMinutes(1) }
            };

            m_SbClient = new ServiceBusClient(Credentials.Current.ConnStr, opts);

            if (numberOfMessages <= 100)
                await SendMessagesAsync(numberOfMessages, queueName);
            else
                await SendMessageBatchAsync(numberOfMessages, queueName);

            Console.WriteLine("Press any key to start receiver...");
            Console.ReadKey();
            Console.ForegroundColor = ConsoleColor.Yellow;

            await RunMessageReceiverWithProcesor(queueName, tokenSource.Token);

            Console.ReadKey();
        }

        /// <summary>
        /// Send message by message to the queue.
        /// </summary>
        /// <param name="numberOfMessagesToSend"></param>
        /// <returns></returns>
        static async Task SendMessagesAsync(int numberOfMessagesToSend, string queueName)
        {
            try
            {
                var sender = m_SbClient.CreateSender(queueName);

                for (var i = 0; i < numberOfMessagesToSend; i++)
                {
                    var message = new ServiceBusMessage(createMessage());
                    message.ApplicationProperties.Add("USECASE", "QueueSamples");
                    message.TimeToLive = TimeSpan.FromMinutes(10);

                    Console.WriteLine($"Sending message: {message.Body}");

                    // Send the message to the queue.
                    await sender.SendMessageAsync(message);
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
        static async Task SendMessageBatchAsync(int numberOfMessagesToSend, string queueName)
        {
            try
            {
                var sender = m_SbClient.CreateSender(queueName);

                using (ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync())
                {
                    for (var i = 0; i < numberOfMessagesToSend; i++)
                    {
                        // Create a new message to send to the queue.
                        var message = new ServiceBusMessage(createMessage());
                        message.ApplicationProperties.Add("USECASE", "QueueSamples");
                        message.TimeToLive = TimeSpan.FromMinutes(10);

                        if (!messageBatch.TryAddMessage(message))
                        {
                            Console.WriteLine($"Batch is full!");

                            break;
                        }
                        else
                            Console.WriteLine($"Adding message to batch: {i}");
                    }

                    // Send batch of messages to the queue.
                    await sender.SendMessagesAsync(messageBatch);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"{DateTime.Now} : Exception: {exception.Message}");
            }
        }

        private static ServiceBusProcessor processor;

        /// <summary>
        /// Register two handlers: Message Receive- and Error-handler.
        /// </summary>
        static async Task RunMessageReceiverWithProcesor(string queueName, CancellationToken token)
        {
            // Configure the message handler options in terms of exception handling, number of concurrent messages to deliver, etc.
            var options = new ServiceBusProcessorOptions
            {
                // Maximum number of concurrent calls to the callback ProcessMessagesAsync(), set to 1 for simplicity.
                // Set it according to how many messages the application wants to process in parallel.
                MaxConcurrentCalls = 1,

                // Indicates whether the message pump should automatically complete the messages after returning from user callback.
                // False below indicates the complete operation is handled by the user callback as in ProcessMessagesAsync().
                AutoCompleteMessages = true,

                ReceiveMode = ServiceBusReceiveMode.PeekLock
            };

            processor = m_SbClient.CreateProcessor(queueName, options);

            processor.ProcessMessageAsync += ProcessMessagesAsync;
            processor.ProcessErrorAsync += ProcessMessageErrorAsync;       

            await processor.StartProcessingAsync(token);
        }


        /// <summary>
        /// Demonstrates how to process message in a not reliable way. If the complete is not called,
        /// message remains in a queue for LockTime duration.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        static async Task ProcessMessagesAsync(ProcessMessageEventArgs args)
        {
            Transfer msg = JsonConvert.DeserializeObject<Transfer>(Encoding.UTF8.GetString(args.Message.Body));

            Console.WriteLine($"From:{msg.fromAccount}, To:{msg.toAccount}, Amount:{msg.amount} EUR");

            await Task.FromResult<bool>(true);
        }

        /// <summary>
        /// Invoked in a case of an error.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static Task ProcessMessageErrorAsync(ProcessErrorEventArgs args)
        {
            // the error source tells me at what point in the processing an error occurred
            Console.WriteLine(args.ErrorSource);
            // the fully qualified namespace is available
            Console.WriteLine(args.FullyQualifiedNamespace);
            // as well as the entity path
            Console.WriteLine(args.EntityPath);
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }

        internal static byte[] createMessage()
        {
            var obj = new Transfer() { fromAccount = "DE123", toAccount = "DE456", amount = (Decimal)1000000.0 };

            string jsonObj = JsonConvert.SerializeObject(obj);

            byte[] binData = Encoding.UTF8.GetBytes(jsonObj);

            return binData;
        }

    }
}
