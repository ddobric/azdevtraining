using Microsoft.Azure.Relay;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static async Task RunAsync(string[] args)
        {
            Console.WriteLine("Enter lines of text to send to the server with ENTER");

            // Create a new hybrid connection client
            var client = new HybridConnectionClient("Endpoint=sb://daenettrainingrelay.servicebus.windows.net/;SharedAccessKeyName=training;SharedAccessKey=oJdG65EwTNwsQtHLTNTbsELoEM5Zqiq/ZmA1UK3DllI=;EntityPath=daenet-hyco");

            // Initiate the connection
            var relayConnection = await client.CreateConnectionAsync();


            // We run two conucrrent loops on the connection. One 
            // reads input from the console and writes it to the connection 
            // with a stream writer. The other reads lines of input from the 
            // connection with a stream reader and writes them to the console. 
            // Entering a blank line will shut down the write task after 
            // sending it to the server. The server will then cleanly shut down
            // the connection will will terminate the read task.

            var reads = Task.Run(async () =>
            {
                while (true)
                {
                    // initialize the stream reader over the connection
                    try
                    {

                        var reader = new StreamReader(relayConnection);
                        var writer = Console.Out;
                        do
                        {

                            Console.ForegroundColor = ConsoleColor.Green;
                            // read a full line of UTF-8 text up to newline
                            string line = await reader.ReadLineAsync();
                            // if the string is empty or null, we are done.
                            if (String.IsNullOrEmpty(line))
                                break;
                            // write to the console
                            await writer.WriteLineAsync(line);

                        }
                        while (true);
                    }
                    catch (Exception ex)
                    {

                    }
                }
            });

            // Read from the console and write to the hybrid connection
            var writes = Task.Run(async () =>
            {
                var reader = Console.In;
                var writer = new StreamWriter(relayConnection) { AutoFlush = true };
                do
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    // read a line form the console
                    string line = await reader.ReadLineAsync();
                    // write the line out, also when it's empty
                    await writer.WriteLineAsync(line);
                    // quit when the line was empty
                    if (String.IsNullOrEmpty(line))
                        break;
                }
                while (true);
            });

            // wait for both tasks to complete
            await Task.WhenAll(reads, writes);
            await relayConnection.CloseAsync(CancellationToken.None);
        }

        static void Main(string[] args)
        {
            RunAsync(args).GetAwaiter().GetResult();
        }
    }
}
