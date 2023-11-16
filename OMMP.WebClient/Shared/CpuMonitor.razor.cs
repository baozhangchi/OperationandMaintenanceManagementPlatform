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

public sealed partial class CpuMonitor : IMonitorComponent
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
        if (AutoRefresh) LineChart?.Update(ChartAction.AddData);
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
        if (string.IsNullOrWhiteSpace(GlobalCache.Instance.CurrentClient.ClientApiUrl)) return dataSource;

        var url = MaxMinDateTimeRangeValue != null
            ? @"api/Cpu/byTimePeriods/{MaxMinDateTimeRangeValue.Start:yyyy-MM-dd HH:mm:ss}/{MaxMinDateTimeRangeValue.End:yyyy-MM-dd HH:mm:ss}"
            : _lastTime.HasValue
                ? $"api/Cpu/{_lastTime.Value:yyyy-MM-dd HH:mm:ss}/1000"
                : $"api/Cpu/1000";
        var data = await HttpHelper.GetAsync<List<CpuLog>>(url);
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
}