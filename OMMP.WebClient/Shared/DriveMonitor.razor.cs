using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Web;
using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using OMMP.Common;
using OMMP.Models;
using OMMP.WebClient.Hubs;
using Console = System.Console;
using Timer = System.Timers.Timer;

namespace OMMP.WebClient.Shared;

public partial class DriveMonitor : IMonitorComponent, IDisposable
{
    private string _drive;
    private bool _refreshDataSignaler;
    private Timer _timer;
    [CascadingParameter(Name = "ClientId")] private string ClientId { get; set; }
    [Inject] [NotNull] private IHubContext<MonitoringHub> HubContext { get; set; }

    [Parameter]
    public string Drive
    {
        get => _drive;
        set
        {
            if (SetField(ref _drive, value)) PieChart?.Reload();
        }
    }

    public Chart PieChart { get; set; }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        return true;
    }

    protected override void OnInitialized()
    {
        if (AutoRefresh)
        {
            _timer = new Timer();
            _timer.Interval = TimeSpan.FromSeconds(5).TotalMilliseconds;
            _timer.Elapsed += (s, e) => { PieChart?.Update(ChartAction.Update); };
            _timer.Start();
        }

        base.OnInitialized();
    }

    private void OnTimerElapsed()
    {
        if (AutoRefresh) PieChart?.Update(ChartAction.Update);
    }

    private async Task<ChartDataSource> InitAsync()
    {
        var dataSource = new ChartDataSource();
        dataSource.Options.Title = $"{Drive} 使用率";
        dataSource.Options.ShowXScales = false;
        dataSource.Options.Colors.Clear();
        dataSource.Options.Colors.Add("Free", "#119c60");
        dataSource.Options.Colors.Add("Used", "red");
        if (string.IsNullOrWhiteSpace(Drive)) return dataSource;

        // var url = $"api/Drive/latest/{Drive.ToBase64()}";
        // var data = await HttpHelper.GetAsync<DriveLog>(url);
        var data = await HubContext.Clients.Client(ClientId)
            .InvokeAsync<DriveLog>(nameof(IMonitoringClientHub.GetPartitionLog), Drive, CancellationToken.None);

        if (data == null) return dataSource;

        dataSource.Labels = new[] { nameof(DriveLog.Free), nameof(DriveLog.Used) };
        dataSource.Data.Add(new ChartDataset()
        {
            ShowPointStyle = false,
            PointRadius = 1,
            PointStyle = ChartPointStyle.Circle,
            // PointHoverRadius = 10,
            Tension = 0,
            BorderWidth = 1,
            Data = new object[] { data.Free, data.Used }
        });

        return dataSource;
    }

    public async Task Reload()
    {
        if (PieChart != null)
            await PieChart.Reload();
    }

    [Parameter] public bool AutoRefresh { get; set; }
    [Parameter] public DateTimeRangeValue MaxMinDateTimeRangeValue { get; set; }

    public void Dispose()
    {
        if (_timer != null)
        {
            _timer.Stop();
            _timer.Dispose();
        }
    }
}