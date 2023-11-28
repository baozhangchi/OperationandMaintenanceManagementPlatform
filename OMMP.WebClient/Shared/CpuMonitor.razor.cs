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

public sealed partial class CpuMonitor : IMonitorComponent, IDisposable
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

    [Parameter] public DateTimeRangeValue MaxMinDateTimeRangeValue { get; set; }

    private async Task<ChartDataSource> OnInit()
    {
        var dataSource = new ChartDataSource();
        dataSource.Options.Title = "CPU监控";
        dataSource.Options.X.Title = "时间";
        dataSource.Options.Y.Title = "使用率";
        dataSource.Options.ShowXScales = false;
        dataSource.Options.ShowLegend = false;

        var arg = MaxMinDateTimeRangeValue != default
            ? new QueryLogArgs(MaxMinDateTimeRangeValue.Start, MaxMinDateTimeRangeValue.End)
            : _lastTime.HasValue
                ? new QueryLogArgs(_lastTime.Value)
                : new QueryLogArgs(1000);
        var data = await HubContext.Clients.Client(ClientId)
            .InvokeAsync<List<CpuLog>>(nameof(IMonitoringClientHub.GetCpuLogs), arg, CancellationToken.None);
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
            Label = $"CPU",
            Data = data.Select(x => (object)(x.Used * 100))
        });

        return dataSource;
    }

    public async Task Reload()
    {
        if (LineChart != null)
            await LineChart.Reload();
    }

    [Parameter] public bool AutoRefresh { get; set; }

    public void Dispose()
    {
        if (_timer != null)
        {
            _timer.Stop();
            _timer.Dispose();
        }
    }
}