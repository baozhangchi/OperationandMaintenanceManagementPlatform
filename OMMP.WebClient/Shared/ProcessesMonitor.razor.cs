using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
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

public partial class ProcessesMonitor : IMonitorComponent, IDisposable
{
    private bool _refreshDataSignaler;
    private DateTime? _lastTime;
    private Timer _timer;
    [CascadingParameter(Name = "ClientId")] private string ClientId { get; set; }
    [Inject] [NotNull] private IHubContext<MonitoringHub> HubContext { get; set; }

    public Chart LineChart { get; set; }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        return true;
    }

    private async Task<ChartDataSource> InitAsync()
    {
        var dataSource = new ChartDataSource();
        dataSource.Options.Title = "系统进程数";
        dataSource.Options.X.Title = "时间";
        dataSource.Options.Y.Title = "进程数";
        dataSource.Options.ShowXScales = false;
        dataSource.Options.ShowLegend = false;

        var arg = MaxMinDateTimeRangeValue != default
            ? new QueryLogArgs(MaxMinDateTimeRangeValue.Start, MaxMinDateTimeRangeValue.End)
            : _lastTime.HasValue
                ? new QueryLogArgs(_lastTime.Value)
                : new QueryLogArgs(1000);
        var data = await HubContext.Clients.Client(ClientId)
            .InvokeAsync<List<ServerResourceLog>>(nameof(IMonitoringClientHub.GetServerResourceLogs), arg,
                CancellationToken.None);
        if (!data.Any()) return dataSource;

        dataSource.Labels = data.Select(x => x.Time.ToString("yyyy-MM-dd HH:mm:ss")).ToList();
        if (AutoRefresh) _lastTime = data.Max(x => x.Time);
        dataSource.Data.Add(new ChartDataset()
        {
            ShowPointStyle = false,
            PointRadius = 1,
            PointStyle = ChartPointStyle.Circle,
            // PointHoverRadius = 10,
            Tension = 0,
            BorderWidth = 1,
            Data = data.Select(x => (object)x.ProcessCount)
        });

        return dataSource;
    }

    protected override void OnInitialized()
    {
        if (AutoRefresh)
        {
            _timer = new Timer();
            _timer.Interval = TimeSpan.FromSeconds(5).TotalMilliseconds;
            _timer.Elapsed += (s, e) => { LineChart?.Update(ChartAction.AddData); };
            _timer.Start();
        }

        base.OnInitialized();
    }

    private void OnTimerElapsed()
    {
        if (AutoRefresh) LineChart?.Update(ChartAction.AddData);
    }

    public async Task Reload()
    {
        if (LineChart != null)
            await LineChart.Reload();
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