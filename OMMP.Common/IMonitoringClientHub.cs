using OMMP.Models;

namespace OMMP.Common;

public class QueryLogArgs
{
    public QueryLogArgs()
    {
    }

    public QueryLogArgs(int count)
    {
        Count = count;
    }

    public QueryLogArgs(DateTime startTime)
    {
        StartTime = startTime;
    }

    public QueryLogArgs(DateTime startTime, DateTime endTime)
    {
        StartTime = startTime;
        EndTime = endTime;
    }

    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int? Count { get; set; }
}

public interface IMonitoringClientHub
{
    Task<List<ApplicationInfo>> GetApplications();

    Task<List<ApplicationLog>> GetApplicationLogs(long applicationId, QueryLogArgs queryLogArgs);

    Task<bool> SaveApplication(ApplicationInfo application);

    Task<List<CpuLog>> GetCpuLogs(QueryLogArgs args);

    Task<List<MemoryLog>> GetMemoryLogs(QueryLogArgs args);

    Task<Dictionary<string, List<NetworkRateLog>>> GetNetworkRateLogs(QueryLogArgs args);

    Task<List<ServerResourceLog>> GetServerResourceLogs(QueryLogArgs args);

    Task<DriveLog> GetPartitionLog(string drive);

    Task<List<string>> GetPartitions();

    Task UpdateApplication(ReadOnlyMemory<byte> buffer);

    Task<ReadOnlyMemory<byte>> GetApplicationBackup();

    Task<ReadOnlyMemory<byte>> GetApplicationLogs();
}