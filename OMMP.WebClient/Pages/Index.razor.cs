using System.Runtime.CompilerServices;

namespace OMMP.WebClient.Pages;

public partial class Index
{
    private MonitoringClient CurrentClient { get; set; }
    private bool RefreshDataSignaler { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        GlobalCache.Instance.CurrentClientChanged -= OnCurrentClientChanged;
        GlobalCache.Instance.TimerElapsed -= OnTimerElapsed;
        CurrentClient = GlobalCache.Instance.CurrentClient;
        GlobalCache.Instance.CurrentClientChanged += OnCurrentClientChanged;
        GlobalCache.Instance.TimerElapsed += OnTimerElapsed;
    }

    private void OnTimerElapsed()
    {
        RefreshDataSignaler = !RefreshDataSignaler;
    }

    private void OnCurrentClientChanged(MonitoringClient client)
    {
        CurrentClient = client;
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        return true;
    }
}