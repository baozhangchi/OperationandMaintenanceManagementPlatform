using System.Runtime.CompilerServices;
using BootstrapBlazor.Components;

namespace OMMP.WebClient.Shared;

public partial class MainLayout
{
    
    private SelectedItem _selectedClient;
    // private Timer _timer;

    public SelectedItem SelectedClient
    {
        get => _selectedClient;
        set
        {
            if (SetField(ref _selectedClient, value))
            {
                GlobalCache.Instance.CurrentClient = GlobalCache.Instance.MonitoringClients.SingleOrDefault(x => x.ClientIpAddress == value.Text);
                GlobalCache.Instance.Timer?.Stop();
                GlobalCache.Instance.Timer?.Start();
            }
        }
    }

    private IEnumerable<SelectedItem> Clients { get; set; }

    public bool RefreshDataSignaler { get; set; }
    public string? SideWidth { get; set; } = @"220px";

    protected override void OnInitialized()
    {
        base.OnInitialized();
        Clients = GlobalCache.Instance.MonitoringClients.Select(x => new SelectedItem(x.ClientApiUrl, x.ClientIpAddress)).ToList();
        SelectedClient = Clients.FirstOrDefault();
        GlobalCache.Instance.TimerElapsed -= RefreshClients;
        GlobalCache.Instance.TimerElapsed += RefreshClients;
        GlobalCache.Instance.Timer.Start();
    }

    private void RefreshClients()
    {
        Clients = GlobalCache.Instance.MonitoringClients.Select(x => new SelectedItem(x.ClientApiUrl, x.ClientIpAddress)).ToList();
        if (!Clients.Contains(SelectedClient))
        {
            SelectedClient = Clients.FirstOrDefault();
        }
        if (SelectedClient != null)
        {
            RefreshDataSignaler = !RefreshDataSignaler;
        }
        InvokeAsync(StateHasChanged);
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        return true;
    }

    private async Task OnCollapsed(bool arg)
    {
        SideWidth = arg ? "0px" : "220px";
        await InvokeAsync(StateHasChanged);
    }
}