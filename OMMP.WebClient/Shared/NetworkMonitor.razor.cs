using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;
using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using OMMP.Models;
using Console = System.Console;
using Timer = System.Timers.Timer;

namespace OMMP.WebClient.Shared;

public sealed partial class NetworkMonitor : IMonitorComponent
{
    private bool _refreshDataSignaler;
    private DateTime? _lastTime;
    public Chart LineChart { get; set; }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        return true;
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        GlobalCache.Instance.TimerElapsed -= OnTimerElapsed;
        GlobalCache.Instance.TimerElapsed += OnTimerElapsed;
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

        var url = MaxMinDateTimeRangeValue != null
            ? @"api/Network/byTimePeriods/{MaxMinDateTimeRangeValue.Start:yyyy-MM-dd HH:mm:ss}/{MaxMinDateTimeRangeValue.End:yyyy-MM-dd HH:mm:ss}"
            : _lastTime.HasValue
                ? $"api/Network/{_lastTime.Value:yyyy-MM-dd HH:mm:ss}/1000"
                : $"api/Network/1000";
        var items = await HttpHelper.GetAsync<Dictionary<string, List<NetworkRateLog>>>(url);
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
}