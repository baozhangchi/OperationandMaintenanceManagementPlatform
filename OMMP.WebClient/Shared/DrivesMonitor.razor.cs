using System.Runtime.CompilerServices;
using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Console = System.Console;

namespace OMMP.WebClient.Shared;

public partial class DrivesMonitor : IMonitorComponent
{
    private async Task LoadDrives()
    {
        var data = await HttpHelper.GetAsync<List<string>>($"api/Drive/partitions");
        Drives = data;
        await InvokeAsync(StateHasChanged);
    }

    protected override async Task OnInitializedAsync()
    {
        await LoadDrives();
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