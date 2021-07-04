using System.Collections.Generic;
using System.Threading.Tasks;

namespace CosmosPerfTests
{
    public interface ISample
    {

    }

    public interface ISample<T> : ISample
    {
        void Dispose();
        Task<List<T>> GetAllTelemetryData();
        Task SaveTelemetryData(T task);
        Task SaveTelemetryData(T[] tasks);
        Task DeleteRecordAsync(T record);
        Task DeleteRecordAsync(T[] telemetryData);
    }
}