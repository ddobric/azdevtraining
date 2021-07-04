using MongoDB.Bson;

namespace CosmosPerfTests
{
    public class TelemetryDocDb
    {
        public string Region { get; set; }

        public string id { get; set; }

        public string DeviceId { get; internal set; }

        public double Temperature { get; internal set; }
    }
}