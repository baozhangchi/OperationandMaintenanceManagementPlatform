using Microsoft.AspNetCore.SignalR.Client;
using OAMMP.Models;

namespace OAMMP.Common;

public interface IClientHandler
{
    HubConnection? Connection { get; set; }
}

public interface IMonitorClientHandler : IClientHandler
{
    Task GetApplications(string taskId);
    Task GetApplicationAlive(string taskId, long appId);
    Task GetApplication(string taskId, long appId);
    Task GetApplicationLogs(string taskId, long applicationId, QueryLogArgs queryLogArgs);
    Task SaveApplication(string taskId, ApplicationItem application);
    Task GetCpuLogs(string taskId, QueryLogArgs args);
    Task GetMemoryLogs(string taskId, QueryLogArgs args);
    Task GetNetworkLogs(string taskId, QueryLogArgs args);
    Task GetServerResourceLogs(string taskId, QueryLogArgs args);
    Task GetPartitionLog(string taskId, string drive);
    Task GetPartitions(string taskId);
    Task UploadApplication(string taskId, ReadOnlyMemory<byte> buffer);
    Task GetApplicationBackup(string taskId);
    Task DownloadApplicationLogs(string taskId);
}

public class QueryLogArgs
{
    public QueryLogArgs(string taskId)
    {
        TaskId = taskId;
    }

    public QueryLogArgs(int count, string taskId)
    {
        Count = count;
        TaskId = taskId;
    }

    public QueryLogArgs(DateTime startTime, string taskId)
    {
        StartTime = startTime;
        TaskId = taskId;
    }

    public QueryLogArgs(DateTime startTime, DateTime endTime, string taskId)
    {
        StartTime = startTime;
        EndTime = endTime;
        TaskId = taskId;
    }

    public string TaskId { get; }

    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int? Count { get; set; }
}