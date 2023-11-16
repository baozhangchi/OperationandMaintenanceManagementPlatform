using System.Collections;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Timer = System.Timers.Timer;

namespace OMMP.WebClient;

public class GlobalCache
{
    private MonitoringClient _currentClient;
    public Timer Timer { get; }

    private GlobalCache()
    {
        Timer = new Timer();
        Timer.Interval = TimeSpan.FromSeconds(5).TotalMilliseconds;
        Timer.Elapsed += (_, _) =>
        {
            Timer.Stop();
            TimerElapsed?.Invoke();
            Timer.Start();
        };
    }

    public static GlobalCache Instance { get; } = new();

    /// <summary>
    /// 监控客户端
    /// </summary>
    public MonitoringClientData MonitoringClients { get; } = new();

    public MonitoringClient CurrentClient
    {
        get => _currentClient;
        set
        {
            if (SetField(ref _currentClient, value))
            {
                CurrentClientChanged?.Invoke(value);
            }
        }
    }

    public delegate void CurrentClientChangedHandler(MonitoringClient client);

    public delegate void TimerElapsedHandler();

    public event TimerElapsedHandler TimerElapsed;

    public event CurrentClientChangedHandler CurrentClientChanged;

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        return true;
    }
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