#region

using Microsoft.AspNetCore.SignalR;
using IClientHandler = OAMMP.Client.Common.IClientHandler;

#endregion

namespace OAMMP.Server.Hubs;

public class ClientHub : Hub
{
    private readonly IMonitorState _monitorState;
    private readonly IClientState _clientState;

    public ClientHub(IServiceProvider serviceProvider)
    {
        _monitorState = serviceProvider.GetRequiredService<IMonitorState>();
        _clientState = serviceProvider.GetRequiredService<IClientState>();
    }

    public override async Task OnConnectedAsync()
    {
        _clientState.Clients = Clients;
        await _clientState.Clients.Caller.SendAsync(nameof(IClientHandler.ClientsUpdated), _monitorState.ToList());
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _clientState.Clients = Clients;
        return base.OnDisconnectedAsync(exception);
    }
}