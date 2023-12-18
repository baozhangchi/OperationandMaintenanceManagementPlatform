using OAMMP.Models;

namespace OAMMP.Client.Common;

public delegate void ApplicationsUpdatedEventHandler(string clientId, List<ApplicationItem> applicationItems);

public delegate void MonitorServersUpdatedEventHandler(List<MonitorServer> monitorServers);

public interface IClientHandler
{
	Task ApplicationsUpdated(string clientId, List<ApplicationItem> applicationItems);

	Task ClientsUpdated(List<MonitorServer> monitorServers);

	Task SetOnApplicationsUpdated(Action<string, List<ApplicationItem>> action);
	Task SetOnMonitorServersUpdated(Action<List<MonitorServer>> action);
}

public class ClientHandler : IClientHandler
{
	Task IClientHandler.ApplicationsUpdated(string clientId, List<ApplicationItem> applicationItems)
	{
		_onApplicationsUpdated?.Invoke(clientId, applicationItems);
		return Task.CompletedTask;
	}

	Task IClientHandler.ClientsUpdated(List<MonitorServer> monitorServers)
	{
		_onMonitorServersUpdated?.Invoke(monitorServers);
		return Task.CompletedTask;
	}

	Task IClientHandler.SetOnApplicationsUpdated(Action<string, List<ApplicationItem>> action)
	{
		_onApplicationsUpdated = action;
		return Task.CompletedTask;
	}

	Task IClientHandler.SetOnMonitorServersUpdated(Action<List<MonitorServer>> action)
	{
		_onMonitorServersUpdated = action;
		return Task.CompletedTask;
	}

	private Action<string, List<ApplicationItem>>? _onApplicationsUpdated;
	private Action<List<MonitorServer>>? _onMonitorServersUpdated;
}