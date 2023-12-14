#region

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
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
}