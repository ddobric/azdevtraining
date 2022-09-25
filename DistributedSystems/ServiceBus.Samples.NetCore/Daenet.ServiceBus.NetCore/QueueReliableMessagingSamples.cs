using Azure.Messaging.ServiceBus;
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

            await RunMessageReceiverWithoutProcessor(queueName, tokenSource.Token);

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
                    message.ApplicationProperties.Add("USECASE", "Reliable Messaging");
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
                        string messageBody = $"Message {i}";
                        var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(messageBody));
                        message.ApplicationProperties.Add("USECASE", "Reliable Messaging");
                        message.TimeToLive = TimeSpan.FromMinutes(10);

                        if (!messageBatch.TryAddMessage(message))
                        {
                            Console.WriteLine($"Batch is full!");

                            break;
                        }
                        else
                            Console.WriteLine($"Adding message to batch: {messageBody}");
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



        /// <summary>
        /// Demonstrates how to process message in a reliable way. If the complete is not called,
        /// message remains in a queue for LockTime duration.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        static async Task RunMessageReceiverWithoutProcessor(string queueName, CancellationToken token)
        {
            var receiver = m_SbClient.CreateReceiver(queueName, new ServiceBusReceiverOptions { ReceiveMode = ServiceBusReceiveMode.PeekLock });

            while (!token.IsCancellationRequested)
            { 
                var message = await receiver.ReceiveMessageAsync(cancellationToken:token);
                if (message == null)
                    break;

                // Process the message.
                Console.WriteLine($"Received message: SequenceNumber:{message.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");

                var json = Encoding.UTF8.GetString(message.Body);

                Transfer transferObj = JsonConvert.DeserializeObject<Transfer>(json);

                if (transferObj.amount < 10000)
                {
                    Console.WriteLine($"Ammount: {transferObj.amount}, From:{transferObj.fromAccount}, To: {transferObj.toAccount}.");

                    // Complete the message so that it is not received again.
                    // This can be done only if the subscriptionClient is created in ReceiveMode.PeekLock mode (which is the default).
                    await receiver.CompleteMessageAsync(message);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Ammount: {transferObj.amount}, From:{transferObj.fromAccount}, To: {transferObj.toAccount}.");
                    await receiver.AbandonMessageAsync(message);
                    Console.ResetColor();
                }
            }
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
