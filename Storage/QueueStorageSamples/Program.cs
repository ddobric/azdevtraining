using System;
using System.Threading.Tasks;

namespace QueueStorageSamples
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello Azure Queues World!");

            QueueStorageSamples samples = new QueueStorageSamples("DefaultEndpointsProtocol=https;AccountName=damiraztraining;AccountKey=EyazJ87KOZG/E8oWTtQxm6DkF4yc0WNF7FvlF4TFHwt9Cl0Z0GQ8UJjiQXtL5zEj/DTLu4KlnUlb1asomxpg3A==;EndpointSuffix=core.windows.net");

            //await samples.CreateQueue();

            //await samples.SendMessages();

            //await samples.ReceiveMessages();

            //await samples.SendReceiveMessagesAdvanced();

            //await samples.DeleteQueue();

            await samples.SendManyMessages();

            //await samples.ReceiveManyMessages();

        }
    }
}
