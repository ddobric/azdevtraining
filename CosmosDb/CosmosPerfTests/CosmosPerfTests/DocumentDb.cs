using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Performance tips:
// https://stackoverflow.com/questions/41744582/fastest-way-to-insert-100-000-records-into-documentdb
// https://docs.microsoft.com/en-us/azure/cosmos-db/indexing-policies
// Requests Unit Calculator: https://www.documentdb.com/capacityplanner

namespace CosmosPerfTests
{
    public class DocumentDb : ISample<TelemetryDocDb>, IDisposable
    {
        private Container container;

        public DocumentDb()
        {          
            var client = new CosmosClient(Credentials.DocumentDb.EndpointUri, Credentials.DocumentDb.Key);

            var database = client.GetDatabase(Credentials.DocumentDb.DatabaseName);

            using (database.GetContainer(Credentials.DocumentDb.ContainerName).DeleteContainerStreamAsync().Result)
            { }

                ContainerProperties containerProperties = new ContainerProperties(Credentials.DocumentDb.ContainerName, partitionKeyPath: "/DeviceId");

            // Create with a throughput of 1000 RU/s
            container = database.CreateContainerIfNotExistsAsync(
                containerProperties).Result;
        }

        /// <summary>
        /// Deletes the single record.
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public async Task DeleteRecordAsync(TelemetryDocDb record)
        {
            ItemResponse<TelemetryDocDb> response = await container.DeleteItemAsync<TelemetryDocDb>(
              partitionKey: new PartitionKey("DeviceId"),
               id: record.id);
        }


        /// <summary>
        /// Deletes list of records.
        /// </summary>
        /// <param name="telemetryData"></param>
        /// <returns></returns>
        public async Task DeleteRecordAsync(TelemetryDocDb[] telemetryData)
        {
            foreach (var doc in telemetryData)
            {
                ItemResponse<TelemetryDocDb> response = await container.DeleteItemAsync<TelemetryDocDb>(
                partitionKey: new PartitionKey("DeviceId"),
                 id: doc.id);
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task<List<TelemetryDocDb>> GetAllTelemetryData()
        {
            QueryDefinition query = new QueryDefinition("select * from container1");

            List<TelemetryDocDb> events = new List<TelemetryDocDb>();

            using (FeedIterator<TelemetryDocDb> resultSet = container.GetItemQueryIterator<TelemetryDocDb>(
                query,
                requestOptions: new QueryRequestOptions()
                {
                    //PartitionKey = new PartitionKey("DeviceId"),
                    MaxItemCount = 1000
                }))
            {
                while (resultSet.HasMoreResults)
                {
                    FeedResponse<TelemetryDocDb> response = await resultSet.ReadNextAsync();
                    TelemetryDocDb telEvent = response.First();
                    Console.WriteLine($"{telEvent.id}; Id: {telEvent.Temperature};");
                    if (response.Diagnostics != null)
                    {
                        Console.WriteLine($" Diagnostics {response.Diagnostics.ToString()}");
                    }

                    foreach (var d in events)
                    {
                        Console.WriteLine($"{d.DeviceId}\t{d.Temperature}");
                    }
                }
            }
            return events;

        }

        /// <summary>
        /// Demonstrates how to filter data and return all possible properties like etag, _rid, _self etc..
        /// </summary>
        public async Task QueryData()
        {

            QueryDefinition query = new QueryDefinition(
               "select * from container1 s where s.Temperature > @temp ")
               .WithParameter("@temp", 32);

            List<TelemetryDocDb> events = new List<TelemetryDocDb>();

            using (FeedIterator<TelemetryDocDb> resultSet = container.GetItemQueryIterator<TelemetryDocDb>(
                query,
                requestOptions: new QueryRequestOptions()
                {
                    //PartitionKey = new PartitionKey("DeviceId"),
                    MaxItemCount = 1
                }))
            {
                while (resultSet.HasMoreResults)
                {
                    FeedResponse<TelemetryDocDb> response = await resultSet.ReadNextAsync();
                    TelemetryDocDb telEvent = response.First();
                    Console.WriteLine($"{telEvent.id}; Id: {telEvent.Temperature};");
                    if (response.Diagnostics != null)
                    {
                        Console.WriteLine($" Diagnostics {response.Diagnostics.ToString()}");
                    }

                    foreach (var d in events)
                    {
                        Console.WriteLine($"{d.DeviceId}\t{d.Temperature}");
                    }
                }

            }
        }


        public async Task SaveTelemetryData(TelemetryDocDb telemetryEvent)
        {
            //creates the  document. 
            ItemResponse<TelemetryDocDb> response = await container.UpsertItemAsync(
                partitionKey: new PartitionKey(telemetryEvent.DeviceId),
                item: telemetryEvent);

            //
            // Notice the response.StatusCode returned indicates a Create operation was performed
            TelemetryDocDb upserted = response.Resource;
            Console.WriteLine($"Request charge of upsert operation: {response.RequestCharge}");
            Console.WriteLine($"StatusCode of this operation: { response.StatusCode}");
        }

        public async Task SaveTelemetryData(TelemetryDocDb[] events)
        {
            foreach (var telemetryEvent in events)
            {
                ItemResponse<TelemetryDocDb> response = await container.UpsertItemAsync(
                           partitionKey: new PartitionKey(telemetryEvent.DeviceId),
                           item: telemetryEvent);
            }
        }

        private bool disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                }
            }

            this.disposed = true;
        }


    }
}
