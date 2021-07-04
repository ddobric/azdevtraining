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

            public const string connectionString = "mongodb://cosmos-mongoapi:pHCkW4U23NL6ILP55KmOC3iqmUueo6iPhDmdK9arZFw4EqJs02hspOsih2kH24eToxL3CSujhtcjRmUgyuHmrg==@cosmos-mongoapi.documents.azure.com:10255/?ssl=true&replicaSet=globaldb";
            public const string UserName = "cosmos-mongoapi";
            public const string Host = "cosmos-mongoapi.documents.azure.com";
            public const string Password = "pHCkW4U23NL6ILP55KmOC3iqmUueo6iPhDmdK9arZFw4EqJs02hspOsih2kH24eToxL3CSujhtcjRmUgyuHmrg==";
        }

        public static class DocumentDb
        {
            public const string EndpointUri = "https://servicebook-dev.documents.azure.com:443/";
            public const string Key = "LgAUVgq0rco2W3ALmH2cyubtxL2KA4Z6rp9eHdghzX9iYeyUJ41DjasTGm2qaQUpDnpZ8WgtRYAhsYQCZv5kXQ==";
            public const string DatabaseName = "ToDoList";
            public const string CollectionName = "Items";
        }
    }
}
