using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Daenet.ServiceBus.NetCore
{
    internal class TopicSessionSample
    {
      
        const string m_TopicName = "topicsamples/sendreceive";

        static ITopicClient topicClient;

        const string m_SubscriptionName = "respSub";//"sys2";

        static ISubscriptionClient m_SubscriptionClient;

        static ISessionClient m_SessionClient;

        public static async Task RunAsync(int numberOfMessages)
        {
            topicClient = new TopicClient(Credentials.Current.ConnStr, m_TopicName);

            m_SubscriptionClient = new SubscriptionClient(Credentials.Current.ConnStr, m_TopicName, m_SubscriptionName, receiveMode: ReceiveMode.PeekLock);
            m_SessionClient = new SessionClient(Credentials.Current.ConnStr, $"{m_TopicName}/Subscriptions/{TopicSample.cSubscriptionName1}", receiveMode: ReceiveMode.PeekLock);

            await SendMessagesAsync(numberOfMessages, "S1");
            await SendMessagesAsync(numberOfMessages, "S2");
            await SendMessagesAsync(numberOfMessages, "S3");

            Console.WriteLine("Press any key to start receiver...");
            Console.ReadKey();
            Console.ForegroundColor = ConsoleColor.Yellow;

            RunReceivers();

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
                    message.UserProperties.Add("USECASE", "UC-2.1-REQ");
                    message.TimeToLive = TimeSpan.FromMinutes(1);
                    if (sessId != null)
                        message.SessionId = sessId;

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


        static void RunReceivers()
        {
            List<Task> tasks = new List<Task>();

            for (int i = 0; i < 3; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    // Register the function that processes messages.
                    var session = await m_SessionClient.AcceptMessageSessionAsync();
                  
                    while (true)
                    {
                        var message = await session.ReceiveAsync();
                        if (message != null)
                        {
                            Console.WriteLine($"Received message: SessionId:{message.SessionId}, SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");
                            await session.CompleteAsync(message.SystemProperties.LockToken);
                        }
                        else
                        {
                            await session.CloseAsync();
                            // TODO. We do need here try catch if no session found.
                            session = await m_SessionClient.AcceptMessageSessionAsync();
                            Console.WriteLine("no messages..");
                        }
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());
        }


        static async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            Console.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");

            // Complete the message so that it is not received again.
            // This can be done only if the subscriptionClient is created in ReceiveMode.PeekLock mode (which is the default).
            await m_SubscriptionClient.CompleteAsync(message.SystemProperties.LockToken);
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
