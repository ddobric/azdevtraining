using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
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

        /// <summary>
        /// Deletes the single record.
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public async Task DeleteRecordAsync(TelemetryDocDb record)
        {
            var client = getClient();

            var docUri = UriFactory.CreateDocumentUri(Credentials.DocumentDb.DatabaseName, Credentials.DocumentDb.ContainerName, record.id);

            // Use without partition key if partionion key is not set.
            //var book = await client.DeleteDocumentAsync(docUri/*, new RequestOptions { PartitionKey = new PartitionKey(1) }*/);

            // If partitioning is used 
            var book = await client.DeleteDocumentAsync(docUri, new RequestOptions { PartitionKey = new PartitionKey(Program.PartitionKey1) });
        }


        /// <summary>
        /// Deletes list of records.
        /// </summary>
        /// <param name="telemetryData"></param>
        /// <returns></returns>
        public async Task DeleteRecordAsync(TelemetryDocDb[] telemetryData)
        {
            List<Task> tasks = new List<Task>();

            foreach (var doc in telemetryData)
            {
                var docUri = UriFactory.CreateDocumentUri(Credentials.DocumentDb.DatabaseName, Credentials.DocumentDb.ContainerName, doc.id);

                //tasks.Add(client.DeleteDocumentAsync(docUri/*));

                tasks.Add(client.DeleteDocumentAsync(docUri, new RequestOptions { PartitionKey = new PartitionKey(Program.PartitionKey1) }));
            }

            Task.WaitAll(tasks.ToArray());
            //var client = getClient();

            //foreach (var doc in telemetryData)
            //{             
            //    var docUri = UriFactory.CreateDocumentUri(Credentials.DocumentDb.DatabaseName, Credentials.DocumentDb.CollectionName, doc.id);

            //    var book = await client.DeleteDocumentAsync(docUri/*, new RequestOptions { PartitionKey = new PartitionKey(1) }*/);

            //}
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public Task<List<TelemetryDocDb>> GetAllTelemetryData()
        {
            return Task<List<TelemetryDocDb>>.Run(() =>
            {
                var client = getClient();

                FeedOptions queryOptions = new FeedOptions { MaxItemCount = 500, EnableCrossPartitionQuery = true };

                IQueryable<TelemetryDocDb> records = client.CreateDocumentQuery<TelemetryDocDb>(
                   UriFactory.CreateDocumentCollectionUri(Credentials.DocumentDb.DatabaseName, Credentials.DocumentDb.ContainerName),
                   $"SELECT * FROM c",
                   queryOptions);

                return records.ToList();
            });
        }

        /// <summary>
        /// Demonstrates how to filter data and return all possible properties like etag, _rid, _self etc..
        /// </summary>
        internal async Task QueryData()
        {
            var client = getClient();

            FeedOptions queryOptions = new FeedOptions { MaxItemCount = 10, EnableCrossPartitionQuery = true, EnableScanInQuery = true, };

            IQueryable<TelemetryDocDb> query = client.CreateDocumentQuery<TelemetryDocDb>(
               UriFactory.CreateDocumentCollectionUri(Credentials.DocumentDb.DatabaseName, Credentials.DocumentDb.ContainerName), queryOptions);

            var filtered = query.Where(b => b.Temperature > 32);

            var docQuery = filtered.AsDocumentQuery();

            while (docQuery.HasMoreResults)
            {
                var docs = await docQuery.ExecuteNextAsync();

                foreach (var d in docs)
                {
                    Console.WriteLine($"{d.DeviceId}\t{d.Temperature}");
                }
            }
        }

        /// <summary>
        /// Demonstrates how to filter data and return specific properties only
        /// </summary>
        internal async Task QueryData3()
        {
            var client = getClient();

            FeedOptions queryOptions = new FeedOptions
            {
                MaxItemCount = 500,
                EnableCrossPartitionQuery = true,
                EnableScanInQuery = true,
            };

            var query = client.CreateDocumentQuery<TelemetryDocDb>(
                UriFactory.CreateDocumentCollectionUri(Credentials.DocumentDb.DatabaseName, Credentials.DocumentDb.ContainerName),
                queryOptions)
                    .Select(d => new TelemetryDocDb { Temperature = d.Temperature, DeviceId = d.DeviceId })
                    .Where(d => d.Temperature > 32)
                    .AsDocumentQuery();

            while (query.HasMoreResults)
            {
                // Note this returns dynamic.
                var docs = await query.ExecuteNextAsync();

                foreach (var doc in docs)
                {
                    Console.WriteLine($"{doc.DeviceId}\t{doc.Temperature}");
                }
            }
        }

        /// <summary>
        /// This sample does not map correctlly TelemetryDocDb properties.
        /// </summary>
        /// <returns></returns>
        internal async Task QueryData2()
        {
            var client = getClient();

            FeedOptions queryOptions = new FeedOptions
            {
                MaxItemCount = 500,
                EnableCrossPartitionQuery = true,
                EnableScanInQuery = true,
            };
            
            var query = client.CreateDocumentQuery<TelemetryDocDb>(
                UriFactory.CreateDocumentCollectionUri(Credentials.DocumentDb.DatabaseName, Credentials.DocumentDb.ContainerName),
                queryOptions)
                    .Select(d => new TelemetryDocDb { id = d.id, Temperature = d.Temperature, DeviceId = d.DeviceId })
                    .Where(d => d.Temperature > 32)
                    .AsDocumentQuery();

            while (query.HasMoreResults)
            {
                var docs = await query.ExecuteNextAsync<TelemetryDocDb>();

                foreach (var doc in docs)
                {
                    Console.WriteLine($"{doc.DeviceId}\t{doc.Temperature}");
                }
            }
        }

        public async Task SaveTelemetryData(TelemetryDocDb telemetryEvent)
        {
            var client = getClient();
            var res = await client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(Credentials.DocumentDb.DatabaseName, Credentials.DocumentDb.ContainerName), telemetryEvent);
        }

        public async Task SaveTelemetryData(TelemetryDocDb[] events)
        {
            var client = getClient();
            foreach (var telemetryEvent in events)
            {
                var res = await client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(Credentials.DocumentDb.DatabaseName, Credentials.DocumentDb.ContainerName), telemetryEvent);
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

        DocumentClient client;// = new DocumentClient(new Uri(Credentials.DocumentDb.EndpointUri),
                              //Credentials.DocumentDb.Key);

        private DocumentClient getClient()
        {
            //var jsonSerializerSettings = new JsonSerializerSettings
            //{
            //    ContractResolver = new CamelCasePropertyNamesContractResolver()
            //};

            //DocumentClient client = new DocumentClient(new Uri(Credentials.DocumentDb.EndpointUri),
            //    
            client = new DocumentClient(new Uri(Credentials.DocumentDb.EndpointUri),
               Credentials.DocumentDb.Key/* jsonSerializerSettings*/);

            return client;
        }
    }
}
