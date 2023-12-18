using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;

namespace OAMMP.Server.Hubs;

public class MonitorHub : Hub
{
	private readonly IClientState _clientState;
	private readonly IMonitorState _monitorState;

	public MonitorHub(IServiceProvider serviceProvider)
	{
		_monitorState = serviceProvider.GetRequiredService<IMonitorState>();
		_clientState = serviceProvider.GetRequiredService<IClientState>();
	}

	private string GetClientIpAddress()
	{
		return Context.GetHttpContext()!.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
	}

	public override Task OnConnectedAsync()
	{
		var clientIpAddress = GetClientIpAddress();
		var apiUrl = Context.GetHttpContext()!.Request.Query["url"].ToString();
		_monitorState[clientIpAddress] = apiUrl;

		_monitorState.Clients = Clients;
		if (_clientState.Clients != null) _clientState.Clients.All.ClientsUpdated(_monitorState.ToList());

		return Task.CompletedTask;
	}

	public override Task OnDisconnectedAsync(Exception? exception)
	{
		var clientIpAddress = GetClientIpAddress();
		if (!string.IsNullOrWhiteSpace(clientIpAddress))
		{
			Console.WriteLine($"客户端{clientIpAddress}已断开连接");
			_monitorState.Remove(clientIpAddress);
		}

		_monitorState.Clients = Clients;
		if (_clientState.Clients != null) _clientState.Clients.All.ClientsUpdated(_monitorState.ToList());

		return Task.CompletedTask;
	}
}