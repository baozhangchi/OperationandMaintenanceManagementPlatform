using Microsoft.AspNetCore.SignalR;

namespace OMMP.WebClient;

public class MonitoringHub : Hub
{
    // public async Task UpdateDataServer(object data)
    //     => await Clients.All.Register();

    public async Task RegisterClient(string url)
    {
        var clientIpAddress = GetClientIpAddress();
        if (!string.IsNullOrWhiteSpace(clientIpAddress))
        {
            Console.WriteLine($"客户端{clientIpAddress}已连接，客户端接口地址为：{url}");
            GlobalCache.Instance.MonitoringClients[clientIpAddress] = url;
        }

        await Task.CompletedTask;
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        var clientIpAddress = GetClientIpAddress();
        if (!string.IsNullOrWhiteSpace(clientIpAddress))
        {
            Console.WriteLine($"客户端{clientIpAddress}已断开连接");
            GlobalCache.Instance.MonitoringClients.Remove(clientIpAddress);
        }

        return base.OnDisconnectedAsync(exception);
    }

    private string GetClientIpAddress()
    {
        return Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
    }
}