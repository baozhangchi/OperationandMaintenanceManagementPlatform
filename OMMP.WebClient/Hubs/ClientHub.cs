using Microsoft.AspNetCore.SignalR;
using OMMP.WebClient.States;

namespace OMMP.WebClient.Hubs;

public class ClientHub : Hub
{
    private readonly IState _state;

    public ClientHub(IServiceProvider serviceProvider)
    {
        _state = serviceProvider.GetRequiredService<IState>();
    }

    public override async Task OnConnectedAsync()
    {
        _state.Clients = Clients;
        await _state.Clients.Caller.SendAsync("ClientsUpdated", _state.ToDictionary(x => x.Key, x => x.Value));
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        _state.Clients = Clients;
        return base.OnDisconnectedAsync(exception);
    }
}