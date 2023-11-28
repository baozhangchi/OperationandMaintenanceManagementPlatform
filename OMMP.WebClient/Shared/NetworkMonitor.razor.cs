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

public sealed partial class NetworkMonitor : IMonitorComponent, IDisposable
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
        dataSource.Options.Title = "网络监控";
        dataSource.Options.X.Title = "时间";
        dataSource.Options.Y.Title = "速率";
        dataSource.Options.ShowXScales = false;
        dataSource.Options.LegendPosition = ChartLegendPosition.Top;

        var arg = MaxMinDateTimeRangeValue != default
            ? new QueryLogArgs(MaxMinDateTimeRangeValue.Start, MaxMinDateTimeRangeValue.End)
            : _lastTime.HasValue
                ? new QueryLogArgs(_lastTime.Value)
                : new QueryLogArgs(1000);
        var items = await HubContext.Clients.Client(ClientId)
            .InvokeAsync<Dictionary<string, List<NetworkRateLog>>>(nameof(IMonitoringClientHub.GetNetworkRateLogs), arg, CancellationToken.None);
        if (!items.Any())
        {
            return dataSource;
        }
        var data = items.ToDictionary(x => x.Key, x => x.Value.Select(_ => new
        {
            Time = new DateTime(_.Time.Year, _.Time.Month, _.Time.Day, _.Time.Hour, _.Time.Minute,
                _.Time.Second),
            _.Down,
            _.Up
        }).ToList());

        if (AutoRefresh)
        {
            _lastTime = data.SelectMany(x => x.Value).Max(x => x.Time);
        }

        dataSource.Labels =
            data.SelectMany(x => x.Value.Select(_ => _.Time))
                .Distinct().OrderBy(x => x).Select(x => x.ToString("yyyy-MM-dd HH:mm:ss")).ToList();
        foreach (var item in data)
        {
            dataSource.Data.Add(new ChartDataset()
            {
                ShowPointStyle = false,
                PointRadius = 1,
                PointStyle = ChartPointStyle.Circle,
                // PointHoverRadius = 10,
                Tension = 0,
                BorderWidth = 1,
                Label = $"{item.Key} Down",
                Data = item.Value.Select(x => (object)x.Down)
            });
            dataSource.Data.Add(new ChartDataset()
            {
                ShowPointStyle = false,
                PointRadius = 1,
                PointStyle = ChartPointStyle.Circle,
                // PointHoverRadius = 10,
                Tension = 0,
                BorderWidth = 1,
                Label = $"{item.Key} Up",
                Data = item.Value.Select(x => (object)x.Up)
            });
        }

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