using Microsoft.AspNetCore.SignalR;
using OMMP.WebClient.States;

namespace OMMP.WebClient.Hubs;

public class MonitoringHub : Hub
{
    // private readonly IServiceProvider _provider;
    private readonly IClientState _clientState;
    private readonly IMonitorState _monitorState;

    public MonitoringHub(IServiceProvider serviceProvider)
    {
        _monitorState = serviceProvider.GetRequiredService<IMonitorState>();
        _clientState = serviceProvider.GetRequiredService<IClientState>();
    }

    public override async Task OnConnectedAsync()
    {
        var clientIpAddress = GetClientIpAddress();
        if (!string.IsNullOrWhiteSpace(clientIpAddress))
        {
            _monitorState[clientIpAddress] = Context.ConnectionId;
        }

        if(_clientState.Clients!=null)
        {
            await _clientState.Clients.All.SendAsync("ClientsUpdated", _monitorState.ToDictionary());
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var clientIpAddress = GetClientIpAddress();
        if (!string.IsNullOrWhiteSpace(clientIpAddress))
        {
            Console.WriteLine($"客户端{clientIpAddress}已断开连接");
            _monitorState.Remove(clientIpAddress);
        }

        if(_clientState.Clients!=null)
        {
            await _clientState.Clients.All.SendAsync("ClientsUpdated", _monitorState.ToDictionary());
        }
    }

    private string GetClientIpAddress()
    {
        return Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
    }
}