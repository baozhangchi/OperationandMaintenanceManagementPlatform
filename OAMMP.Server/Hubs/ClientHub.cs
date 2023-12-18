#region

using Microsoft.AspNetCore.SignalR;
using IClientHandler = OAMMP.Client.Common.IClientHandler;

#endregion

namespace OAMMP.Server.Hubs;

public class ClientHub : Hub<IClientHandler>
{
	private readonly IMonitorState _monitorState;
	private readonly IClientState _clientState;

	public ClientHub(IServiceProvider serviceProvider)
	{
		_monitorState = serviceProvider.GetRequiredService<IMonitorState>();
		_clientState = serviceProvider.GetRequiredService<IClientState>();
	}

	public override Task OnConnectedAsync()
	{
		_clientState.Clients = Clients;
		_clientState.Clients.Caller.ClientsUpdated(_monitorState.ToList());
		return Task.CompletedTask;
	}

	public override Task OnDisconnectedAsync(Exception? exception)
	{
		_clientState.Clients = Clients;
		return base.OnDisconnectedAsync(exception);
	}
}