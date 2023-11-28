using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Timers;
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

public partial class MemoryMonitor : IMonitorComponent, IDisposable
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
        if (AutoRefresh)
        {
            LineChart?.Update(ChartAction.AddData);
        }
    }

    private async Task<ChartDataSource> OnInit()
    {
        var dataSource = new ChartDataSource();
        dataSource.Options.Title = "内存使用率";
        // dataSource.Options.LegendLabelsFontSize = 16;
        dataSource.Options.X.Title = "时间";
        dataSource.Options.Y.Title = "使用率%";
        dataSource.Options.ShowXScales = false;
        dataSource.Options.ShowLegend = false;
        if (string.IsNullOrWhiteSpace(ClientId))
        {
            return dataSource;
        }

        var arg = MaxMinDateTimeRangeValue != default
            ? new QueryLogArgs(MaxMinDateTimeRangeValue.Start, MaxMinDateTimeRangeValue.End)
            : _lastTime.HasValue
                ? new QueryLogArgs(_lastTime.Value)
                : new QueryLogArgs(1000);
        var data = await HubContext.Clients.Client(ClientId)
            .InvokeAsync<List<MemoryLog>>(nameof(IMonitoringClientHub.GetMemoryLogs), arg, CancellationToken.None);
        if (!data.Any())
        {
            return dataSource;
        }

        dataSource.Labels = data.Select(x => x.Time.ToString("yyyy-MM-dd HH:mm:ss")).ToList();
        _lastTime = data.Max(x => x.Time);
        dataSource.Data.Add(new ChartDataset()
        {
            ShowPointStyle = false,
            PointRadius = 1,
            PointStyle = ChartPointStyle.Circle,
            // PointHoverRadius = 10,
            Tension = 0,
            BorderWidth = 1,
            // Label = $"Memory",
            Data = data.Select(x => (object)((double)x.Used / (double)x.Total * 100))
        });

        return dataSource;
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