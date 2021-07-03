using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace QueueStorageSamples
{
    public class QueueStorageSamples
    {
        private string ConnectionString { get; set; }

        public QueueStorageSamples(string connStr)
        {
            this.ConnectionString = connStr;
        }

        /// <summary>
        /// Get a random name so we won't have any conflicts when creating
        /// resources.
        /// </summary>
        /// <param name="prefix">Optional prefix for the random name.</param>
        /// <returns>A random name.</returns>
        public string Randomize(string prefix = "sample") =>
            $"{prefix}-{Guid.NewGuid()}";

        public async Task CreateQueue(string queueName = "queue-name")
        {
            QueueClient queue = new QueueClient(ConnectionString, queueName);

            await queue.CreateIfNotExistsAsync();

            await queue.CreateAsync();
        }

        public async Task SendMessages(string queueName = "queue-name")
        {
            QueueClient queue = new QueueClient(ConnectionString, queueName);

            await queue.SendMessageAsync("Hello, Azure Queues!");

            var json = JsonConvert.SerializeObject(new MyMessage() { Name = Randomize("SomeName"), EMail = $"{Randomize("mail")}@daenet.com" });

            await queue.SendMessageAsync(json);
        }

        public async Task SendReceiveMessagesAdvanced(string queueName = "queue-name")
        {
            QueueClient queue = new QueueClient(ConnectionString, queueName);

            var json = JsonConvert.SerializeObject(new MyMessage() { Name = Randomize("SomeName"), EMail = $"{Randomize("mail")}@daenet.com" });

            await queue.SendMessageAsync(json, timeToLive: TimeSpan.FromMinutes(2), visibilityTimeout: TimeSpan.FromMinutes(1));

            var msg = await queue.ReceiveMessageAsync();

            msg = await queue.ReceiveMessageAsync(visibilityTimeout: TimeSpan.FromSeconds(15));

        }

        public async Task PeekMessages(string queueName = "queue-name")
        {
            QueueClient queue = new QueueClient(ConnectionString, queueName);

            var msgs = await queue.PeekMessagesAsync(maxMessages: 10);

            foreach (PeekedMessage message in msgs.Value)
            {
                // "Process" the message
                Console.WriteLine($"Message: {message.MessageText}");
            }

        }


        public async Task ReceiveMessages(string queueName = "queue-name")
        {
            QueueClient queue = new QueueClient(ConnectionString, queueName);

            foreach (QueueMessage message in queue.ReceiveMessages(maxMessages: 10).Value)
            {
                // "Process" the message
                Console.WriteLine($"Message: {message.MessageText}");

                if (!message.MessageText.Contains("Hello"))
                {
                    MyMessage msg = JsonConvert.DeserializeObject<MyMessage>(message.MessageText);
                    Console.WriteLine($"Name: {msg.Name}, email: {msg.EMail}");
                }

                // Let the service know we're finished with the message and
                // it can be safely deleted.
                await queue.DeleteMessageAsync(message.MessageId, message.PopReceipt);
            }

        }


        public async Task DeleteQueue(string queueName = "queue-name")
        {
            QueueClient queue = new QueueClient(ConnectionString, queueName);

            await queue.DeleteAsync();
        }

        public async Task SendManyMessages(string queueName = "queue-name")
        {
            QueueClient queue = new QueueClient(ConnectionString, queueName);

            for (int i = 0; i < 1000; i++)
            {
                await queue.SendMessageAsync($"cnt: {i} - Hello {DateTime.Now.Ticks}");
                Console.WriteLine($"Sending message {i}");

                await Task.Delay(500);
            }          
        }

        public async Task ReceiveManyMessages(string queueName = "queue-name")
        {
            
            QueueClient queue = new QueueClient(ConnectionString, queueName);

            while (true)
            {
                Console.WriteLine("Start receiving of messages...");

                foreach (QueueMessage message in queue.ReceiveMessages().Value)
                {
                    // "Process" the message
                    Console.WriteLine($"Message: {message.MessageText}");

                    // Let the service know we're finished with the message and
                    // it can be safely deleted.
                    await queue.DeleteMessageAsync(message.MessageId, message.PopReceipt);
                }

                await Task.Delay(5000);
            }
        }
    }
}
