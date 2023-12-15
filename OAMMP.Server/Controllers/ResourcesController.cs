#region

using Microsoft.AspNetCore.Mvc;
using OAMMP.Common;
using OAMMP.Models;
using OAMMP.Server.Hubs;

#endregion

namespace OAMMP.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ResourcesController : ControllerBase
{
	private readonly IMonitorState _state;

	public ResourcesController(IMonitorState state)
	{
		_state = state;
	}

	[HttpPost("cpu/{connectionId}")]
	public Task<List<CpuLog>?> GetCpuLogs(string connectionId, [FromBody] QueryLogArgs args)
	{
		return _state.InvokeAsync<List<CpuLog>>(connectionId, nameof(IMonitorClientHandler.GetCpuLogs), args);
	}

	[HttpPost("memory/{connectionId}")]
	public Task<List<MemoryLog>?> GetMemoryLogs(string connectionId, [FromBody] QueryLogArgs args)
	{
		return _state.InvokeAsync<List<MemoryLog>>(connectionId, nameof(IMonitorClientHandler.GetMemoryLogs), args);
	}

	//[HttpPost("net/{connectionId}")]
	//public Task<List<NetworkLog>?> GetNetworkLogs(string connectionId, [FromBody] QueryLogArgs args)
	//{
	//	return _state.InvokeAsync<List<NetworkLog>>(connectionId, nameof(IMonitorClientHandler.GetNetworkLogs), args);
	//}

	[HttpPost("net/{connectionId}/{mac}")]
	public Task<List<NetworkLog>?> GetNetworkLogs(string connectionId, string mac, [FromBody] QueryLogArgs args)
	{
		return _state.InvokeAsync<List<NetworkLog>>(connectionId, nameof(IMonitorClientHandler.GetNetworkLogs), mac, args);
	}

	[HttpGet("net/{connectionId}")]
	public Task<Dictionary<string,string>?> GetNetworkCards(string connectionId)
	{
		return _state.InvokeAsync<Dictionary<string, string>>(connectionId, nameof(IMonitorClientHandler.GetNetworkCards));
	}

	[HttpPost("{connectionId}")]
	public Task<List<ServerResourceLog>?> GetServerResourceLogs(string connectionId, [FromBody] QueryLogArgs args)
	{
		return _state.InvokeAsync<List<ServerResourceLog>>(connectionId, nameof(IMonitorClientHandler.GetServerResourceLogs), args);
	}

	[HttpGet("partition/{connectionId}/{drive}")]
	public Task<PartitionLog?> GetPartitionLog(string connectionId, string drive)
	{
		return _state.InvokeAsync<PartitionLog>(connectionId, nameof(IMonitorClientHandler.GetPartitionLog), drive);
	}

	[HttpGet("partitions/{connectionId}")]
	public Task<List<string>?> GetPartitions(string connectionId)
	{
		return _state.InvokeAsync<List<string>>(connectionId, nameof(IMonitorClientHandler.GetPartitions));
	}
}