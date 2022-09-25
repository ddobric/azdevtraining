
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
        
        const string m_QueueName = "queuesamples/sendreceive";

        /// <summary>
        /// Client for sending and receiving queue messages.
        /// </summary>
        static ServiceBusClient m_SbClient;

        //static IQueueClient m_DLClient;

        /// <summary>
        /// Start sending of messages.
        /// </summary>
        /// <param name="numberOfMessages"></param>
        /// <param name="autoComplete">True if messaging is none reliable. Use false for reliable messaging.</param>
        /// <returns></returns>
        public static async Task RunAsync(int numberOfMessages)
        {
            tokenSource = new CancellationTokenSource();

            ServiceBusClientOptions opts = new ServiceBusClientOptions
            {
             RetryOptions = new ServiceBusRetryOptions {  MaxRetries = 3, MaxDelay = TimeSpan.FromMinutes(3), Delay = TimeSpan.FromMinutes(1)}
            };
           
            m_SbClient = new ServiceBusClient(Credentials.Current.ConnStr, opts);

            Console.WriteLine("Press any key to start receiver...");
            Console.ReadKey();

            Console.ForegroundColor = ConsoleColor.Yellow;

            if (numberOfMessages > 100)
                await SendMessageBatchAsync(numberOfMessages);
            else
                await SendMessagesAsync(numberOfMessages);

            await RunReceiver(tokenSource.Token);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Press any key to start receiver...");
            Console.ReadKey();

            RegisterOnMDLQessageHandlerAndReceiveMessages();

            Console.ReadKey();
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
                    // Create a new message to send to the queue.
                    string messageBody = $"Message {i}";
                    //var message = new Message(Encoding.UTF8.GetBytes(messageBody));
                    var message = new ServiceBusMessage(createMessage());
                    message.ApplicationProperties.Add("USECASE", "UC-2.1-REQ");
                    message.TimeToLive = TimeSpan.FromMinutes(10);

                    Console.WriteLine($"Sending message: {messageBody}");

                    var sender = m_SbClient.CreateSender(m_QueueName);

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
        static async Task SendMessageBatchAsync(int numberOfMessagesToSend)
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

                var sender = m_SbClient.CreateSender(m_QueueName);

                // Send the message to the queue.
                await sender.SendMessageAsync(message);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"{DateTime.Now} : Exception: {exception.Message}");
            }
        }


        ///// <summary>
        ///// Register message handlers: Message Receive- and Error-handler.
        ///// </summary>
        //static void RegisterOnMessageHandlerAndReceiveMessages()
        //{
        //    // Configure the message handler options in terms of exception handling, number of concurrent messages to deliver, etc.
        //    var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
        //    {
        //        // Maximum number of concurrent calls to the callback ProcessMessagesAsync(), set to 1 for simplicity.
        //        // Set it according to how many messages the application wants to process in parallel.
        //        MaxConcurrentCalls = 1,

        //        // Indicates whether the message pump should automatically complete the messages after returning from user callback.
        //        // False below indicates the complete operation is handled by the user callback as in ProcessMessagesAsync().
        //        AutoComplete = false
        //    };

        //    // Register the function that processes messages with reliable messaging.
        //    m_SbClient.RegisterMessageHandler(ProcessMessagesReliableAsync, messageHandlerOptions);

        //}

        ///// <summary>
        ///// Register message handlers: Message Receive- and Error-handler.
        ///// </summary>
        //static void RegisterOnMDLQessageHandlerAndReceiveMessages()
        //{
        //    // Configure the message handler options in terms of exception handling, number of concurrent messages to deliver, etc.
        //    var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
        //    {
        //        // Maximum number of concurrent calls to the callback ProcessMessagesAsync(), set to 1 for simplicity.
        //        // Set it according to how many messages the application wants to process in parallel.
        //        MaxConcurrentCalls = 1,

        //        // Indicates whether the message pump should automatically complete the messages after returning from user callback.
        //        // False below indicates the complete operation is handled by the user callback as in ProcessMessagesAsync().
        //        AutoComplete = false
        //    };


        //    // Register the function that processes messages in dead-letter queue.
        //    m_DLClient.RegisterMessageHandler(ProcessDLQAsync, messageHandlerOptions);
        //}

        /// <summary>
        /// Demonstrates how to process message in a reliable way. If the complete is not called,
        /// message remains in a queue for LockTime duration.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        static async Task RunReceiver(CancellationToken token)
        {
            ServiceBusReceiver receiver = m_SbClient.CreateReceiver($"{m_QueueName}/$DeadLetterQueue",
            new ServiceBusReceiverOptions { ReceiveMode = ServiceBusReceiveMode.PeekLock });

            while (token.IsCancellationRequested == false)
            {
                var msg = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(30));

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
        static async Task ProcessDLQAsync(CancellationToken token)
        {
            ServiceBusReceiver receiver = m_SbClient.CreateReceiver($"{m_QueueName}/$DeadLetterQueue",
              new ServiceBusReceiverOptions { ReceiveMode = ServiceBusReceiveMode.PeekLock });


            while (token.IsCancellationRequested == false)
            {
                var msg = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(30));


                // Process the message.
                Console.WriteLine($"Received message: SequenceNumber:{msg.SequenceNumber} Body:{Encoding.UTF8.GetString(msg.Body)}");

                var json = Encoding.UTF8.GetString(msg.Body);

                Transfer transferObj = JsonConvert.DeserializeObject<Transfer>(json);

                Console.WriteLine($"Ammount: {transferObj.amount}, From:{transferObj.fromAccount}, To: {transferObj.toAccount}.");

                await receiver.CompleteMessageAsync(msg);
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
