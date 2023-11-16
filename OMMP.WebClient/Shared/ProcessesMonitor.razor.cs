using System.Runtime.CompilerServices;
using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using OMMP.Models;
using Console = System.Console;
using Timer = System.Timers.Timer;

namespace OMMP.WebClient.Shared;

public partial class ProcessesMonitor : IMonitorComponent
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

    private async Task<ChartDataSource> InitAsync()
    {
        var dataSource = new ChartDataSource();
        dataSource.Options.Title = "系统进程数";
        dataSource.Options.X.Title = "时间";
        dataSource.Options.Y.Title = "进程数";
        dataSource.Options.ShowXScales = false;
        dataSource.Options.ShowLegend = false;

        var url = MaxMinDateTimeRangeValue != null
            ? @"api/Processes/byTimePeriods/{MaxMinDateTimeRangeValue.Start:yyyy-MM-dd HH:mm:ss}/{MaxMinDateTimeRangeValue.End:yyyy-MM-dd HH:mm:ss}"
            : _lastTime.HasValue
                ? $"api/Processes/{_lastTime.Value:yyyy-MM-dd HH:mm:ss}/1000"
                : $"api/Processes/1000";
        var data = await HttpHelper.GetAsync<List<ServerResourceLog>>(url);
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
        base.OnInitialized();
        GlobalCache.Instance.TimerElapsed -= OnTimerElapsed;
        GlobalCache.Instance.TimerElapsed += OnTimerElapsed;
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
}