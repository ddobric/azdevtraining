using MongoDB.Bson;

namespace CosmosPerfTests
{
    public class TelemetryMongo 
    {
        public string Region { get; set; }

        public ObjectId _id { get; set; }

        public string DeviceId { get; internal set; }

        public double Temperature { get; internal set; }
    }
}