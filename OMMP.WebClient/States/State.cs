#region

using System.Collections;
using Microsoft.AspNetCore.SignalR;

#endregion

namespace OMMP.WebClient.States;

public interface IState : IClientState, IMonitorState
{
}

public class State : IState
{
    private readonly Dictionary<string, string> _clientMap = new();

    public string this[string ip]
    {
        get => _clientMap.TryGetValue(ip, out var clientId) ? clientId : null;

        set => _clientMap[ip] = value;
    }

    public void Remove(string ip)
    {
        _clientMap.Remove(ip);
    }

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
        return _clientMap.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)this).GetEnumerator();
    }

    public IHubCallerClients Clients { get; set; }
}