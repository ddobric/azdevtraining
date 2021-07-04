namespace Sender
{
    using System.Configuration;
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using System.Runtime.Serialization.Json;
    using System.Diagnostics;
    using Newtonsoft.Json;

    class Program
    {
        private static string m_ConnStr = "Endpoint=sb://daenettraininghub.servicebus.windows.net/;SharedAccessKeyName=training;SharedAccessKey=PmdcD8Dma/jdWT/3Oeq84ysSQJFh0iXdD5BhT5xgCUk=";

        static void Main(string[] args)
        {
            sendMessages();
        }

        private static void sendMessages()
        {
            Stopwatch stopper = new Stopwatch();
            double elapsesMs = 0;
            long msgCount = 0;

            var eventHubClient = EventHubClient.CreateFromConnectionString(m_ConnStr, "sample");
            Random rnd = new Random();

            while (true)
            {
                int maxCount = 100;

                for (int i = 0; i < maxCount; i++)
                {
                    try
                    {   
                        var msg = new { Time = DateTime.Now, Temperature = rnd.Next(20, 35)};

                        Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, msg);
                        msgCount++;
                        stopper.Reset();
                        stopper.Start();
                        eventHubClient.Send(new EventData(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msg))));
                        stopper.Stop();

                        elapsesMs = (double)(elapsesMs + stopper.ElapsedMilliseconds);
                    }
                    catch (Exception exception)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("{0} > Exception: {1}", DateTime.Now, exception.Message);
                        Console.ResetColor();
                    }
                }

                Console.WriteLine("Sent {0} messages. Average message transfer time {1} ms", msgCount, elapsesMs / (double)msgCount);
                Console.WriteLine("Press any key to send {0} messages", maxCount);
                Console.ReadLine();
            }
        }
    }
}
