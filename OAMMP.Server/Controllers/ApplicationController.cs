#region

using Microsoft.AspNetCore.Mvc;
using OAMMP.Common;
using OAMMP.Models;
using OAMMP.Server.Hubs;

#endregion

namespace OAMMP.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ApplicationController : ControllerBase
{
    private readonly IMonitorState _state;

    public ApplicationController(IMonitorState state)
    {
        _state = state;
    }

    [HttpGet("{clientId}")]
    public Task<List<ApplicationItem>?> GetApplications(string clientId)
    {
        return _state.InvokeAsync<List<ApplicationItem>>(clientId, nameof(IMonitorClientHandler.GetApplications));
    }

    [HttpPost("{clientId}")]
    public Task<bool> SaveApplication(string clientId, [FromBody] ApplicationItem item)
    {
        return _state.InvokeAsync<bool>(clientId, nameof(IMonitorClientHandler.SaveApplication), item);
    }

    [HttpPost("{clientId}/{applicationId}")]
    public Task<List<ApplicationLog>?> GetApplicationLogs(string clientId, long applicationId,
        [FromBody] QueryLogArgs args)
    {
        return _state.InvokeAsync<List<ApplicationLog>>(clientId, nameof(IMonitorClientHandler.GetApplicationLogs),
            applicationId, args);
    }

    [HttpGet("alive/{clientId}/{applicationId}")]
    public Task<bool> GetApplicationAlive(string clientId, long applicationId)
    {
        return _state.InvokeAsync<bool>(clientId, nameof(IMonitorClientHandler.GetApplicationLogs), applicationId);
    }

    [HttpGet("app/{clientId}/{applicationId}")]
    public Task<ApplicationItem?> GetApplication(string clientId, long applicationId)
    {
        return _state.InvokeAsync<ApplicationItem>(clientId, nameof(IMonitorClientHandler.GetApplication),
            applicationId);
    }
}