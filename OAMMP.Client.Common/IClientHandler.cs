using Microsoft.AspNetCore.SignalR.Client;
using OAMMP.Models;

namespace OAMMP.Client.Common;

public delegate void ApplicationsUpdatedEventHandler(string clientId, List<ApplicationItem> applicationItems);

public interface IClientHandler:OAMMP.Common.IClientHandler
{
    Func<List<MonitorServer>, Task>? ClientsUpdatedFunc { get; set; }

    void ApplicationsUpdated(string clientId, List<ApplicationItem> applicationItems);

    public async Task ClientsUpdated(List<MonitorServer> monitorServers)
    {
        if (ClientsUpdatedFunc != null)
        {
            await ClientsUpdatedFunc(monitorServers);
        }
    }

    public event ApplicationsUpdatedEventHandler OnApplicationsUpdated;
}

public class ClientHandler : IClientHandler
{
    public Func<List<MonitorServer>, Task>? ClientsUpdatedFunc { get; set; }

    public void ApplicationsUpdated(string clientId, List<ApplicationItem> applicationItems)
    {
        OnApplicationsUpdated?.Invoke(clientId, applicationItems);
    }

    public event ApplicationsUpdatedEventHandler? OnApplicationsUpdated;

    public HubConnection? Connection { get; set; }
}