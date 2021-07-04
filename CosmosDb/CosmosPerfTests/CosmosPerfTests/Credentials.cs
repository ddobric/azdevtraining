using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security;
using System.Text;

namespace CosmosPerfTests
{
    public static class Credentials
    {
        public static class MongoDb
        {

            public const string ConnectionString = @"mongodb://damirtraining-scosmosdb-mongo:xe8WlzzNgGneNb93QxbDqWrVmFuNVbfxf4eJRgVHoGxA4FAPRdXs4k2Zlx7g0D7hOX0qOih5upUp2hNzOdCdJA==@damirtraining-scosmosdb-mongo.mongo.cosmos.azure.com:10255/?ssl=true&replicaSet=globaldb&retrywrites=false&maxIdleTimeMS=120000&appName=@damirtraining-scosmosdb-mongo@";
            public const string UserName = "";
            public const string Host = "";
            public const string Password = "";
        }

        public static class DocumentDb
        {
            public const string EndpointUri = "https://damirtraining-cosmos-sql.documents.azure.com:443/";
            public const string Key = "KYk8eaRqMd74R7GlVd52lVbTUCG1vARPAnrXQFHRQyRsz5GU6Fyk2NX9S6DkLgStevzb7k4i3mELgQVOxwNK4w==";
            public const string DatabaseName = "db1";
            public const string ContainerName = "container1";
        }
    }
}
