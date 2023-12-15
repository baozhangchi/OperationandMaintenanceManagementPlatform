using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using OAMMP.Client.Common;
using OAMMP.Models;

namespace OAMMP.Server.Hubs;

public class MonitorHub : Hub
{
	private readonly IMonitorState _monitorState;
	private readonly IClientState _clientState;

	public MonitorHub(IServiceProvider serviceProvider)
	{
		_monitorState = serviceProvider.GetRequiredService<IMonitorState>();
		_clientState = serviceProvider.GetRequiredService<IClientState>();
	}

	public Task Callback(string taskId, JToken state)
	{
		_monitorState.CompleteTask(taskId, state);
		return Task.CompletedTask;
	}

	public Task ApplicationsUpdated(List<ApplicationItem> applicationItems)
	{
		if (_clientState.Clients != null)
		{
			return _clientState.Clients.All.SendAsync(nameof(IClientHandler.ApplicationsUpdated), Context.ConnectionId,
				applicationItems, CancellationToken.None);
		}

		return Task.CompletedTask;
	}

	private string GetClientIpAddress()
	{
		return Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
	}

	public override Task OnConnectedAsync()
	{
		var clientIpAddress = GetClientIpAddress();
		if (!string.IsNullOrWhiteSpace(clientIpAddress))
		{
			_monitorState[clientIpAddress] = Context.ConnectionId;
		}

		_monitorState.Clients = Clients;
		if (_clientState.Clients != null)
		{
			return _clientState.Clients.All.SendAsync(nameof(IClientHandler.ClientsUpdated), _monitorState.ToList());
		}

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
		if (_clientState.Clients != null)
		{
			return _clientState.Clients.All.SendAsync(nameof(IClientHandler.ClientsUpdated), _monitorState.ToList());
		}

		return Task.CompletedTask;
	}
}