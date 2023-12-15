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
	Task GetNetworkCards(string taskId);
	Task GetMemoryLogs(string taskId, QueryLogArgs args);
	//Task GetNetworkLogs(string taskId, QueryLogArgs args);
	Task GetNetworkLogs(string taskId, string mac, QueryLogArgs args);
	Task GetServerResourceLogs(string taskId, QueryLogArgs args);
	Task GetPartitionLog(string taskId, string drive);
	Task GetPartitions(string taskId);
	Task UploadApplication(string taskId, ReadOnlyMemory<byte> buffer);
	Task GetApplicationBackup(string taskId);
	Task DownloadApplicationLogs(string taskId);
	Task DeleteApplications(string taskId, List<long> applicationIds);
}

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