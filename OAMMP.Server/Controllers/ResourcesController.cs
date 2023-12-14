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

    [HttpPost("cpu/{clientId}")]
    public Task<List<CpuLog>?> GetCpuLogs(string clientId, [FromBody] QueryLogArgs args)
    {
        return _state.InvokeAsync<List<CpuLog>>(clientId, nameof(IMonitorClientHandler.GetCpuLogs), args);
    }

    [HttpPost("memory/{clientId}")]
    public Task<List<MemoryLog>?> GetMemoryLogs(string clientId, [FromBody] QueryLogArgs args)
    {
        return _state.InvokeAsync<List<MemoryLog>>(clientId, nameof(IMonitorClientHandler.GetMemoryLogs), args);
    }

    [HttpPost("net/{clientId}")]
    public Task<List<NetworkLog>?> GetNetworkLogs(string clientId, [FromBody] QueryLogArgs args)
    {
        return _state.InvokeAsync<List<NetworkLog>>(clientId, nameof(IMonitorClientHandler.GetNetworkLogs), args);
    }

    [HttpPost("net/{clientId}")]
    public Task<List<ServerResourceLog>?> GetServerResourceLogs(string clientId, [FromBody] QueryLogArgs args)
    {
        return _state.InvokeAsync<List<ServerResourceLog>>(clientId, nameof(IMonitorClientHandler.GetServerResourceLogs), args);
    }

    [HttpGet("partition/{clientId}/{drive}")]
    public Task<List<PartitionLog>?> GetPartitionLogs(string clientId, string drive)
    {
        return _state.InvokeAsync<List<PartitionLog>>(clientId, nameof(IMonitorClientHandler.GetPartitionLog), drive);
    }

    [HttpGet("partitions/{clientId}")]
    public Task<List<string>?> GetPartitions(string clientId)
    {
        return _state.InvokeAsync<List<string>>(clientId, nameof(IMonitorClientHandler.GetPartitions));
    }
}