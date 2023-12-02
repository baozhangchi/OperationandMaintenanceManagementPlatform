using Microsoft.AspNetCore.SignalR;
using OMMP.WebClient.States;

namespace OMMP.WebClient.Hubs;

public class MonitoringHub : Hub
{
    private readonly IState _state;

    public MonitoringHub(IServiceProvider serviceProvider)
    {
        _state = serviceProvider.GetRequiredService<IState>();
    }

    public override async Task OnConnectedAsync()
    {
        var clientIpAddress = GetClientIpAddress();
        if (!string.IsNullOrWhiteSpace(clientIpAddress))
        {
            _state[clientIpAddress] = Context.ConnectionId;
        }

        if(_state.Clients!=null)
        {
            await _state.Clients.All.SendAsync("ClientsUpdated", _state.ToDictionary());
        }
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var clientIpAddress = GetClientIpAddress();
        if (!string.IsNullOrWhiteSpace(clientIpAddress))
        {
            Console.WriteLine($"客户端{clientIpAddress}已断开连接");
            _state.Remove(clientIpAddress);
        }

        if(_state.Clients!=null)
        {
            await _state.Clients.All.SendAsync("ClientsUpdated", _state.ToDictionary());
        }
    }

    private string GetClientIpAddress()
    {
        return Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
    }
}