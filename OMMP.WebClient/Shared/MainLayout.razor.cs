using System.Runtime.CompilerServices;
using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace OMMP.WebClient.Shared;

public partial class MainLayout
{
    private SelectedItem _selectedClient;
    private HubConnection _hubConnection;

    public string ClientId { get; set; }

    public SelectedItem SelectedClient
    {
        get => _selectedClient;
        set
        {
            if (SetField(ref _selectedClient, value))
            {
                ClientId = value?.Value;
                InvokeAsync(StateHasChanged);
            }
        }
    }

    private List<SelectedItem> Clients { get; set; }

    public bool RefreshDataSignaler { get; set; }
    public string? SideWidth { get; set; } = @"220px";
    [Inject] private NavigationManager Navigation { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        var url = Navigation.ToAbsoluteUri("/Client").ToString();
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(url)
            .Build();
        _hubConnection.On<Dictionary<string, string>>("ClientsUpdated", UpdateClients);
        await _hubConnection.StartAsync();
    }

    private async Task UpdateClients(Dictionary<string, string> clientMap)
    {
        var selectedClientId = SelectedClient?.Text;
        Clients = clientMap.Select(x => new SelectedItem(x.Value, x.Key)).ToList();
        SelectedClient = Clients.FirstOrDefault(x => string.Equals(x.Text, selectedClientId)) ?? Clients.FirstOrDefault();
        await InvokeAsync(StateHasChanged);
    }

    private void OnMonitoringClientDataRemoved(string ipAddress)
    {
        var item = Clients.FirstOrDefault(x => x.Text == ipAddress);
        if (item != null)
        {
            Clients.Remove(item);
            SelectedClient = Clients.FirstOrDefault();
        }
    }

    private void OnMonitoringClientDataChanged(string ipAddress, string clientId)
    {
        var item = Clients.FirstOrDefault(x => x.Text == ipAddress);
        if (item != null)
        {
            item.Value = clientId;
        }
        else
        {
            Clients.Add(new SelectedItem(clientId, ipAddress));
        }
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