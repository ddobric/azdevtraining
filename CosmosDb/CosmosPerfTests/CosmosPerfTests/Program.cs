using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CosmosPerfTests
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("v1.1");

            if (args != null && args.Length == 1 && args[0].ToLower() == "mongodb")
            {
                ISample<TelemetryMongo> sampleMongo = new MongoSample();

                Console.WriteLine($"MongoDB Test started...");

                RunAsync(sampleMongo).Wait();
            }
            else if (args != null && args.Length == 1 && (args[0].ToLower() == "documentdb" || args[0].ToLower() == "docdb"))
            {
                ISample<TelemetryDocDb> sampleDocDb = new DocumentDb();

                Console.WriteLine($"DocumentDB Test started...");

                RunAsync(sampleDocDb).Wait();
            }
            else
            {
                ISample<TelemetryMongo> sampleMongo = new MongoSample();

                Console.WriteLine($"MongoDB Test started...");

                RunAsync(sampleMongo).Wait();

                ISample<TelemetryDocDb> sampleDocDb = new DocumentDb();

                Console.WriteLine("--------------------------------------");
                Console.WriteLine($"DocumentDB Test started...");

                RunAsync(sampleDocDb).Wait();
            }

            Console.WriteLine($"Test completed. Press any key to exit.");

            Console.ReadLine();

        }

        private static async Task RunAsync<T>(ISample<T> sample) where T : class
        {

            int batchSize = 100;//500, 1000

            //
            // Adding single records
            //

            Stopwatch watch = new Stopwatch();

            Console.WriteLine($"Uploading {batchSize} records ony by one.");

            watch.Start();

            for (int i = 0; i < batchSize; i++)
            {
                var data = CreateSampleData<T>(i);

                await sample.SaveTelemetryData(data);
            }

            watch.Stop();

            Console.WriteLine($"Completed with: {watch.ElapsedMilliseconds}");

            Console.WriteLine($"Getting {batchSize} records");

            watch.Restart();

            //
            // Get all data from store.
            //

            var telemetryData = await sample.GetAllTelemetryData();

            await sample.QueryData();

            watch.Stop();

            Console.WriteLine($"All records returned in: {watch.ElapsedMilliseconds}");


            //
            // Deleting single record
            //

            Console.WriteLine($"Deleting {batchSize} records one by one");

            watch.Restart();

            foreach (var record in telemetryData)
            {
                Console.Write(".");

                await sample.DeleteRecordAsync(record);
            }

            watch.Stop();

            Console.WriteLine();

            Console.WriteLine($"All records deleted in: {watch.ElapsedMilliseconds}");


            //
            // Adding batch of records
            //

            Console.WriteLine($"Uploading {batchSize} records");

            watch.Restart();

            List<T> telList = new List<T>();

            for (int i = 0; i < batchSize; i++)
            {
                telList.Add(CreateSampleData<T>(i));
            }

            await sample.SaveTelemetryData(telList.ToArray());

            watch.Stop();

            Console.WriteLine($"Record batch added in: {watch.ElapsedMilliseconds}");

            Console.WriteLine($"Start returning {batchSize} records.");

            watch.Restart();

            //
            // Deleting of all records
            //
            telemetryData = await sample.GetAllTelemetryData();

            Console.WriteLine($"Returned batch of {batchSize} records in: {watch.ElapsedMilliseconds}");

            Console.WriteLine($"Deleting {batchSize} records at once.");

            watch.Restart();

            await sample.DeleteRecordAsync(telemetryData.ToArray());

            watch.Stop();

            Console.WriteLine($"All records deleted in: {watch.ElapsedMilliseconds}");
        }

        private static Random m_Random = new Random();

        public static string PartitionKey1 = "DE";

        private static T CreateSampleData<T>(int i)
        {
            if (typeof(T) == typeof(TelemetryMongo))
            {
                return (T)(object)new TelemetryMongo()
                {
                    Region = "DE",

                    _id = MongoDB.Bson.ObjectId.GenerateNewId(),

                    DeviceId = $"DEV_{i}",

                    Temperature = m_Random.Next(25, 35) + m_Random.NextDouble(),
                };
            }
            else if (typeof(T) == typeof(TelemetryDocDb))
            {
                return (T)(object)new TelemetryDocDb()
                {
                    id = $"ID_{i}",
                    DeviceId = $"DEV_{i}",
                    Region = PartitionKey1,
                    Temperature = m_Random.Next(25, 35) + m_Random.NextDouble(),
                };
            }
            else
                throw new Exception();
        }
    }
}
