
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Amqp.Framing;
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
    internal class DeadLetterMessagingSamples
    {
        private static CancellationTokenSource tokenSource;
        
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
            tokenSource = new CancellationTokenSource();

            ServiceBusClientOptions opts = new ServiceBusClientOptions
            {
                RetryOptions = new ServiceBusRetryOptions {  MaxRetries = 3, MaxDelay = TimeSpan.FromMinutes(3), Delay = TimeSpan.FromMinutes(1)}
            };
           
            m_SbClient = new ServiceBusClient(Credentials.Current.ConnStr, opts);

            Console.ForegroundColor = ConsoleColor.Yellow;

            if (numberOfMessages > 100)
                await SendMessageBatchAsync(numberOfMessages, queueName);
            else
                await SendMessagesAsync(numberOfMessages, queueName);


            Console.WriteLine("Press any key to start receiver...");

            Console.ReadKey();

            await RunReceiver(queueName, tokenSource.Token);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Press any key to start receiver...");
            Console.ReadKey();

            await RunDeadLetterQueueReceiver(queueName, tokenSource.Token);

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
                    // Create a new message to send to the queue.
                    string messageBody = $"Message {i}";
                    //var message = new Message(Encoding.UTF8.GetBytes(messageBody));
                    var message = new ServiceBusMessage(createMessage());
                    message.ApplicationProperties.Add("USECASE", "UC-2.1-REQ");
                    message.TimeToLive = TimeSpan.FromMinutes(10);

                    Console.WriteLine($"Sending message: {messageBody}");

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
        /// Sends a batch of messages to the queue.
        /// </summary>
        /// <param name="numberOfMessagesToSend"></param>
        /// <returns></returns>
        static async Task SendMessageBatchAsync(int numberOfMessagesToSend, string queueName)
        {
            try
            {
                List<ServiceBusMessage> messages = new List<ServiceBusMessage>();

                for (var i = 0; i < numberOfMessagesToSend; i++)
                {
                    // Create a new message to send to the queue.
                    string messageBody = $"Message {i}";
                    var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(messageBody));
                    message.ApplicationProperties.Add("USECASE", "UC-2.1-REQ");
                    message.TimeToLive = TimeSpan.FromMinutes(10);
                    Console.WriteLine($"Adding message to batch: {messageBody}");
                    messages.Add(message);
                }

                var sender = m_SbClient.CreateSender(queueName);

                // Send messages to the queue.
                await sender.SendMessagesAsync(messages);

                //
                // Another way to send batch of messages is to use ServiceBusMessageBatch.
                //

                using (ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync())
                {
                    foreach (var message in messages)
                    {
                        if (!messageBatch.TryAddMessage(message))
                            break;
                    }
                   
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
        static async Task RunReceiver(string queueName, CancellationToken token)
        {
            ServiceBusReceiver receiver = m_SbClient.CreateReceiver(queueName, new ServiceBusReceiverOptions { ReceiveMode = ServiceBusReceiveMode.PeekLock });

            while (token.IsCancellationRequested == false)
            {
                var msg = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(10));
                if (msg == null)
                    break;

                Console.WriteLine($"Received message: SequenceNumber:{msg.SequenceNumber} Body:{Encoding.UTF8.GetString(msg.Body)}");

                var json = Encoding.UTF8.GetString(msg.Body);

                Transfer transferObj = JsonConvert.DeserializeObject<Transfer>(json);

                if (transferObj.amount > 15000)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"Ammount: {transferObj.amount}, From:{transferObj.fromAccount}, To: {transferObj.toAccount}.");

                    // Complete the message is not required.
                    await receiver.DeadLetterMessageAsync(msg, "Amount too large");

                }
                else if (transferObj.amount < 10000)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Ammount: {transferObj.amount}, From:{transferObj.fromAccount}, To: {transferObj.toAccount}.");

                    // Complete the message so that it is not received again.
                    // This can be done only if the subscriptionClient is created in ReceiveMode.PeekLock mode (which is the default).
                    await receiver.CompleteMessageAsync(msg);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"Ammount: {transferObj.amount}, From:{transferObj.fromAccount}, To: {transferObj.toAccount}.");
                    await receiver.AbandonMessageAsync(msg);

                    Console.ResetColor();
                }
            }
        }

        /// <summary>
        /// Demonstrates how to process message in a reliable way. If the complete is not called,
        /// message remains in a queue for LockTime duration.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        static async Task RunDeadLetterQueueReceiver(string queueName, CancellationToken token)
        {
            ServiceBusReceiver receiver = m_SbClient.CreateReceiver($"{queueName}/$DeadLetterQueue", new ServiceBusReceiverOptions { ReceiveMode = ServiceBusReceiveMode.PeekLock });

            while (token.IsCancellationRequested == false)
            {
                // Demonstrates how to receive a batch of messages.
                var msgs = await receiver.ReceiveMessagesAsync(maxMessages:100, maxWaitTime: TimeSpan.FromSeconds(30));

                foreach (var msg in msgs)
                {
                    // Process the message.
                    Console.WriteLine($"Received message: SequenceNumber:{msg.SequenceNumber} Body:{Encoding.UTF8.GetString(msg.Body)}");

                    var json = Encoding.UTF8.GetString(msg.Body);

                    Transfer transferObj = JsonConvert.DeserializeObject<Transfer>(json);

                    Console.WriteLine($"Ammount: {transferObj.amount}, From:{transferObj.fromAccount}, To: {transferObj.toAccount}.");

                    await receiver.CompleteMessageAsync(msg);
                }
            }
        }

        ///// <summary>
        ///// Invoked in a case of an error.
        ///// </summary>
        ///// <param name="exceptionReceivedEventArgs"></param>
        ///// <returns></returns>
        //static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        //{
        //    Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
        //    var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
        //    Console.WriteLine("Exception context for troubleshooting:");
        //    Console.WriteLine($"- Endpoint: {context.Endpoint}");
        //    Console.WriteLine($"- Entity Path: {context.EntityPath}");
        //    Console.WriteLine($"- Executing Action: {context.Action}");
        //    return Task.CompletedTask;
        //}

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
