using Microsoft.AspNetCore.SignalR;
using OMMP.WebClient.States;

namespace OMMP.WebClient.Hubs;

public class ClientHub : Hub
{
    private readonly IClientState _clientState;
    private readonly IMonitorState _monitorState;

    public ClientHub(IServiceProvider serviceProvider)
    {
        _monitorState = serviceProvider.GetRequiredService<IMonitorState>();
        _clientState = serviceProvider.GetRequiredService<IClientState>();
    }

    public override async Task OnConnectedAsync()
    {
        _clientState.Clients = Clients;
        await _clientState.Clients.Caller.SendAsync("ClientsUpdated", _monitorState.ToDictionary());
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        _clientState.Clients = Clients;
        return base.OnDisconnectedAsync(exception);
    }
}