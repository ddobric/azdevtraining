using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace CosmosPerfTests
{
    public class MongoSample : ISample<TelemetryMongo>, IDisposable
    {
        private bool disposed = false;

        private string dbName = "db1";

        private string collectionName = "collection1";


        //public async Task FileSample()
        //{
        //    string fName = "testfile.jpg";
        //    string newFileName = "downloaded.jpg";

        //    using (var fs = new FileStream(fName, FileMode.Open))
        //    {
        //        var database = getDataBase();

        //        var gridFsInfo = database.GridFS.Upload(fs, fName);
        //        var fileId = gridFsInfo.Id;

        //        ObjectId oid = new ObjectId(fileId);
        //        var file = database.GridFS.FindOne((f) => f.);

        //        using (var stream = file.OpenRead())
        //        {
        //            var bytes = new byte[stream.Length];
        //            stream.Read(bytes, 0, (int)stream.Length);
        //            using (var newFs = new FileStream(newFileName, FileMode.Create))
        //            {
        //                newFs.Write(bytes, 0, bytes.Length);
        //            }
        //        }
        //    }
        //}


        public async Task QueryData()
        {
            var collection = getCollection();

            FilterDefinition<TelemetryMongo> filter = FilterDefinition<TelemetryMongo>.Empty;

            FindOptions<TelemetryMongo> options = new FindOptions<TelemetryMongo>
            {
                BatchSize = 2,
                NoCursorTimeout = false
            };

            var data = await collection.FindAsync(filter, options);

            using (IAsyncCursor<TelemetryMongo> cursor = await collection.FindAsync((doc) => doc.Temperature > 32))
            {
                while (await cursor.MoveNextAsync())
                {
                    IEnumerable<TelemetryMongo> batch = cursor.Current;
                    foreach (TelemetryMongo document in batch)
                    {
                        Console.WriteLine($"_id: {document._id}, t={document.Temperature}, device={document.DeviceId}");
                    }
                }
            }
        }


        // Gets all Task items from the MongoDB server.        
        public async Task<List<TelemetryMongo>> GetAllTelemetryData()
        {
            try
            {
                var collection = getCollection();

                FilterDefinition<TelemetryMongo> filter = FilterDefinition<TelemetryMongo>.Empty;

                FindOptions<TelemetryMongo> options = new FindOptions<TelemetryMongo>
                {
                    BatchSize = 2,
                    NoCursorTimeout = false
                };

                var data = await collection.FindAsync(filter, options);

                return await data.ToListAsync();
                //while (await data.MoveNextAsync())
                //{
                //    IEnumerable<Telemetry> batch = data.Current;
                //    foreach (Telemetry document in batch)
                //    {
                //        Console.WriteLine(document);
                //        Console.WriteLine();
                //    }
                //}


            }
            catch (MongoConnectionException)
            {
                return new List<TelemetryMongo>();
            }
        }

        public async Task DeleteRecordAsync(TelemetryMongo record)
        {
            var collection = getCollection();

            FilterDefinition<TelemetryMongo> filter = FilterDefinition<TelemetryMongo>.Empty;

            FindOptions<TelemetryMongo> options = new FindOptions<TelemetryMongo>
            {
                BatchSize = 2,
                NoCursorTimeout = false
            };

            await collection.DeleteOneAsync<TelemetryMongo>((rec) => rec._id == record._id);
        }


        public async Task DeleteRecordAsync(TelemetryMongo[] records)
        {
            var collection = getCollection();

            FilterDefinition<TelemetryMongo> filter = FilterDefinition<TelemetryMongo>.Empty;

            FindOptions<TelemetryMongo> options = new FindOptions<TelemetryMongo>
            {
                BatchSize = 2,
                NoCursorTimeout = false
            };

            await collection.DeleteManyAsync<TelemetryMongo>((rec) => true);
        }

        // Creates a Task and inserts it into the collection in MongoDB.
        public async Task SaveTelemetryData(TelemetryMongo doc)
        {
            var collection = getCollection();
            try
            {
                await collection.InsertOneAsync(doc);
            }
            catch (MongoCommandException ex)
            {
                string msg = ex.Message;
            }
        }


        public async Task SaveTelemetryData(TelemetryMongo[] tasks)
        {
            var collection = getCollection();
            try
            {
                await collection.InsertManyAsync(tasks);
            }
            catch (MongoCommandException ex)
            {
                string msg = ex.Message;
            }
        }

        private IMongoDatabase getDataBase()
        {
            MongoClient client = new MongoClient(getSettings());
            return  client.GetDatabase(dbName);;
        }

        private IMongoCollection<TelemetryMongo> getCollection()
        {           
            var database = getDataBase();
            var todoTaskCollection = database.GetCollection<TelemetryMongo>(collectionName);
            return todoTaskCollection;
        }

        //private IMongoCollection<Telemetry> GetTasksCollectionForEdit()
        //{
        //    MongoClient client = new MongoClient(getSettings());
        //    var database = client.GetDatabase(dbName);
        //    var todoTaskCollection = database.GetCollection<Telemetry>(collectionName);
        //    return todoTaskCollection;
        //}

        private MongoClientSettings getSettings()
        {
            MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl(Credentials.MongoDb.ConnectionString));
            settings.SslSettings = new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };
            var mongoClient = new MongoClient(settings);

            //MongoClientSettings settings = new MongoClientSettings();
            //settings.Server = new MongoServerAddress(Credentials.MongoDb.Host, 10255);
            //settings.UseSsl = true;
            //settings.SslSettings = new SslSettings();
            //settings.SslSettings.EnabledSslProtocols = SslProtocols.Tls12;
            //MongoIdentity identity = new MongoInternalIdentity(dbName, Credentials.MongoDb.UserName);
            //MongoIdentityEvidence evidence = new PasswordEvidence(Credentials.MongoDb.Password);
            //settings.Credential = new MongoCredential("SCRAM-SHA-1", identity, evidence);

            return settings;
        }
        # region IDisposable

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

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





        #endregion
    }
}