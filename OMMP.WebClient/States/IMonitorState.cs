using System.Collections;

namespace OMMP.WebClient.States;

public interface IMonitorState : IEnumerable<KeyValuePair<string, string>>
{
    string this[string ip] { get; set; }

    void Remove(string ip);
}

class MonitorState : IMonitorState
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
}