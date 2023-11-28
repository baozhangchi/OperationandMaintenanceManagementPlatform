using Microsoft.AspNetCore.SignalR;

namespace OMMP.WebClient.States;

public interface IClientState
{
    IHubCallerClients Clients { get; set; }
}

class ClientState : IClientState
{
    public IHubCallerClients Clients { get; set; }
}