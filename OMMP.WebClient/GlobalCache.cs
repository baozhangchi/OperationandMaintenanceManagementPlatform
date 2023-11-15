using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace OMMP.WebClient;

public class GlobalCache
{
    private GlobalCache()
    {
    }

    public static GlobalCache Instance { get; } = new();

    /// <summary>
    /// 监控客户端
    /// </summary>
    public MonitoringClientData MonitoringClients { get; } = new();
}

public class MonitoringClientData : IEnumerable<MonitoringClient>
{
    internal MonitoringClientData()
    {
    }

    public ObservableCollection<MonitoringClient> MonitoringClients { get; } = new();

    public string this[string ip]
    {
        get { return MonitoringClients.SingleOrDefault(x => x.ClientIpAddress == ip)?.ClientApiUrl; }

        set
        {
            var item = MonitoringClients.SingleOrDefault(x => x.ClientIpAddress == ip);
            if (item == null)
                MonitoringClients.Add(new MonitoringClient() { ClientIpAddress = ip, ClientApiUrl = value });
            else
                item.ClientApiUrl = value;
        }
    }

    public void Remove(string ip)
    {
        var item = MonitoringClients.SingleOrDefault(x => x.ClientApiUrl == ip);
        if (item != null)
        {
            MonitoringClients.Remove(item);
        }
    }

    IEnumerator<MonitoringClient> IEnumerable<MonitoringClient>.GetEnumerator()
    {
        return MonitoringClients.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)this).GetEnumerator();
    }
}

public class MonitoringClient
{
    public string ClientIpAddress { get; set; }

    public string ClientApiUrl { get; set; }
}