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

            public const string ConnectionString = @"mongodb://damirtraining:9CKqPA049GQjU6XRsZHYQIZafFvuoSgGgmChUz5suFC1nGyTR8JErSGVh5mI2ppCjSjxHweUimRO39BxWekZjg==@damirtraining.mongo.cosmos.azure.com:10255/?ssl=true&replicaSet=globaldb&retrywrites=false&maxIdleTimeMS=120000&appName=@damirtraining@";
            public const string UserName = "";
            public const string Host = "";
            public const string Password = "";
        }

        public static class DocumentDb
        {
            public const string EndpointUri = "https://damirtraining-cosmos-sql.documents.azure.com:443/";
            public const string Key = "piawZzalwmuvdXE86zEUTkDruoJT5Mi4yVlKkZuP9Wpdbpu4IwdLRuqX7n6fSPELgZDzwb10OFe5Ec1ntV9ycg==";
            public const string DatabaseName = "db1";
            public const string ContainerName = "container1";
        }
    }
}
