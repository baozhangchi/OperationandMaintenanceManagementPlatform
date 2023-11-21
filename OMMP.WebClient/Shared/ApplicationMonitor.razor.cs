using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;
using OMMP.Models;

namespace OMMP.WebClient.Shared;

public partial class ApplicationMonitor : IMonitorComponent
{
    [Parameter] public bool AutoRefresh { get; set; }
    [Parameter] public DateTimeRangeValue MaxMinDateTimeRangeValue { get; set; }

    private void OnTimerElapsed()
    {
        if (AutoRefresh)
        {
            LineChart?.Update(ChartAction.AddData);
        }
    }

    public async Task Reload()
    {
        if (LineChart != null)
            await LineChart.Reload();
    }

    [Parameter] public ApplicationInfo Application { get; set; }
    private DateTime? _lastTime;
    private DateTimeRange _timeRangeSelector;
    public Chart LineChart { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        GlobalCache.Instance.TimerElapsed -= OnTimerElapsed;
        GlobalCache.Instance.TimerElapsed += OnTimerElapsed;
        AutoRefresh = true;
    }

    private async Task<ChartDataSource> InitAsync()
    {
        var dataSource = new ChartDataSource();
        dataSource.Options.Title = "应用资源监控监控";
        dataSource.Options.X.Title = "时间";
        dataSource.Options.Y.Title = "速率";
        dataSource.Options.ShowXScales = false;
        dataSource.Options.LegendPosition = ChartLegendPosition.Top;

        var url = MaxMinDateTimeRangeValue != null
            ? $@"api/AppLog/byTimePeriods/{Application.UUID}/{MaxMinDateTimeRangeValue.Start:yyyy-MM-dd HH:mm:ss}/{MaxMinDateTimeRangeValue.End:yyyy-MM-dd HH:mm:ss}"
            : _lastTime.HasValue
                ? $"api/AppLog/{Application.UUID}/{_lastTime.Value:yyyy-MM-dd HH:mm:ss}/1000"
                : $"api/AppLog/{Application.UUID}/1000";
        var items = await HttpHelper.GetAsync<List<ApplicationLog>>(url);
        if (!items.Any())
        {
            return dataSource;
        }

        if (AutoRefresh)
        {
            _lastTime = items.Max(x => x.Time);
        }

        dataSource.Labels =
            items.Select(x => x.Time)
                .Distinct().OrderBy(x => x).Select(x => x.ToString("yyyy-MM-dd HH:mm:ss")).ToList();
        dataSource.Data.Add(new ChartDataset()
        {
            ShowPointStyle = false,
            PointRadius = 1,
            PointStyle = ChartPointStyle.Circle,
            // PointHoverRadius = 10,
            Tension = 0,
            BorderWidth = 1,
            Label = $"Cpu (%)",
            Data = items.Select(x => (object)(x.CpuUsed * 100))
        });
        dataSource.Data.Add(new ChartDataset()
        {
            ShowPointStyle = false,
            PointRadius = 1,
            PointStyle = ChartPointStyle.Circle,
            // PointHoverRadius = 10,
            Tension = 0,
            BorderWidth = 1,
            Label = $"Memory (%)",
            Data = items.Select(x => (object)(x.MemoryUsed * 100))
        });
        dataSource.Data.Add(new ChartDataset()
        {
            ShowPointStyle = false,
            PointRadius = 1,
            PointStyle = ChartPointStyle.Circle,
            // PointHoverRadius = 10,
            Tension = 0,
            BorderWidth = 1,
            Label = $"线程数",
            Data = items.Select(x => (object)x.ThreadCount)
        });
        dataSource.Data.Add(new ChartDataset()
        {
            ShowPointStyle = false,
            PointRadius = 1,
            PointStyle = ChartPointStyle.Circle,
            // PointHoverRadius = 10,
            Tension = 0,
            BorderWidth = 1,
            Label = $"平均响应时间",
            Data = items.Select(x => (object)x.AverageResponseTime)
        });
        dataSource.Data.Add(new ChartDataset()
        {
            ShowPointStyle = false,
            PointRadius = 1,
            PointStyle = ChartPointStyle.Circle,
            // PointHoverRadius = 10,
            Tension = 0,
            BorderWidth = 1,
            Label = $"数据库连接数",
            Data = items.Select(x => (object)x.DatabaseConnectionPool)
        });
        dataSource.Data.Add(new ChartDataset()
        {
            ShowPointStyle = false,
            PointRadius = 1,
            PointStyle = ChartPointStyle.Circle,
            // PointHoverRadius = 10,
            Tension = 0,
            BorderWidth = 1,
            Label = $"网络下载速度",
            Data = items.Select(x => (object)x.NetworkDownRate)
        });
        dataSource.Data.Add(new ChartDataset()
        {
            ShowPointStyle = false,
            PointRadius = 1,
            PointStyle = ChartPointStyle.Circle,
            // PointHoverRadius = 10,
            Tension = 0,
            BorderWidth = 1,
            Label = $"网络上传速度",
            Data = items.Select(x => (object)x.NetworkUpRate)
        });
        dataSource.Data.Add(new ChartDataset()
        {
            ShowPointStyle = false,
            PointRadius = 1,
            PointStyle = ChartPointStyle.Circle,
            // PointHoverRadius = 10,
            Tension = 0,
            BorderWidth = 1,
            Label = $"磁盘读取速度",
            Data = items.Select(x => (object)x.IOReadRate)
        });
        dataSource.Data.Add(new ChartDataset()
        {
            ShowPointStyle = false,
            PointRadius = 1,
            PointStyle = ChartPointStyle.Circle,
            // PointHoverRadius = 10,
            Tension = 0,
            BorderWidth = 1,
            Label = $"磁盘写入速度",
            Data = items.Select(x => (object)x.IOWriteRate)
        });
        dataSource.Data.Add(new ChartDataset()
        {
            ShowPointStyle = false,
            PointRadius = 1,
            PointStyle = ChartPointStyle.Circle,
            // PointHoverRadius = 10,
            Tension = 0,
            BorderWidth = 1,
            Label = $"JVM内存",
            Data = items.Select(x => (object)x.JVMMemoryFootprint)
        });

        return dataSource;
    }

    private async Task SearchButtonClick()
    {
        MaxMinDateTimeRangeValue = _timeRangeSelector.Value;
        AutoRefresh = MaxMinDateTimeRangeValue != null || MaxMinDateTimeRangeValue == default;
        if (AutoRefresh)
        {
            _lastTime = null;
        }

        if (LineChart != null)
        {
            await LineChart.Update(ChartAction.Reload);
        }
    }
}