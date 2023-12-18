using Microsoft.AspNetCore.SignalR;
using OAMMP.Client.Common;

namespace OAMMP.Server.Hubs;

public class ClientState : IClientState
{
	public IHubCallerClients<IClientHandler>? Clients { get; set; }
}

public interface IClientState
{
	IHubCallerClients<IClientHandler>? Clients { get; set; }
}