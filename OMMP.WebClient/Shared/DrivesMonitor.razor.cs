using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using OMMP.Common;
using OMMP.WebClient.Hubs;
using Console = System.Console;

namespace OMMP.WebClient.Shared;

public partial class DrivesMonitor : IMonitorComponent
{
    [CascadingParameter(Name = "ClientId")] private string ClientId { get; set; }
    [Inject] [NotNull] private IHubContext<MonitoringHub> HubContext { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (string.IsNullOrWhiteSpace(ClientId))
        {
            return;
        }
        Drives = await HubContext.Clients.Client(ClientId)
            .InvokeAsync<List<string>>(nameof(IMonitoringClientHub.GetPartitions), CancellationToken.None);
    }

    public async Task Reload()
    {
    }

    public List<string> Drives { get; set; }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        return true;
    }

    [Parameter] public bool AutoRefresh { get; set; }
    [Parameter] public DateTimeRangeValue MaxMinDateTimeRangeValue { get; set; }
}