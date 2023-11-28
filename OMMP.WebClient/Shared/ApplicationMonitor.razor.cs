using System.Diagnostics.CodeAnalysis;
using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.JSInterop;
using OMMP.Common;
using OMMP.Models;
using OMMP.WebClient.Hubs;
using Timer = System.Timers.Timer;

namespace OMMP.WebClient.Shared;

public partial class ApplicationMonitor : IMonitorComponent, IDisposable
{
    private Timer _timer;

    [CascadingParameter(Name = "ClientId")]
    private string ClientId { get; set; }

    [Inject] [NotNull] private IHubContext<MonitoringHub> HubContext { get; set; }
    [Inject] private ToastService ToastService { get; set; }
    [Inject] private IJSRuntime JS { get; set; }

    [Inject] private DialogService DialogService { get; set; }
    [Parameter] public bool AutoRefresh { get; set; }
    [Parameter] public DateTimeRangeValue MaxMinDateTimeRangeValue { get; set; }

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
        AutoRefresh = true;
        _timer = new Timer();
        _timer.Interval = TimeSpan.FromSeconds(5).TotalMilliseconds;
        _timer.Elapsed += (s, e) => { LineChart?.Update(ChartAction.AddData); };
        _timer.Start();
    }

    private async Task<ChartDataSource> InitAsync()
    {
        var dataSource = new ChartDataSource();
        dataSource.Options.Title = "应用资源监控监控";
        dataSource.Options.X.Title = "时间";
        dataSource.Options.Y.Title = "速率";
        dataSource.Options.ShowXScales = false;
        dataSource.Options.LegendPosition = ChartLegendPosition.Top;

        if (string.IsNullOrWhiteSpace(ClientId))
        {
            return dataSource;
        }


        var arg = MaxMinDateTimeRangeValue != default
            ? new QueryLogArgs(MaxMinDateTimeRangeValue.Start, MaxMinDateTimeRangeValue.End)
            : _lastTime.HasValue
                ? new QueryLogArgs(_lastTime.Value)
                : new QueryLogArgs(1000);
        var items = await HubContext.Clients.Client(ClientId)
            .InvokeAsync<List<ApplicationLog>>(nameof(IMonitoringClientHub.GetApplicationLogs), Application.UUID,
                arg, CancellationToken.None);

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

    public void Dispose()
    {
        if (_timer != null)
        {
            _timer.Stop();
            _timer.Dispose();
        }
    }

    private async Task OnClickToUpload(UploadFile file)
    {
        var result = await HubContext.Clients.Client(ClientId).InvokeAsync<bool>(
            nameof(IMonitoringClientHub.UpdateApplication),
            await file.GetBytesAsync(), CancellationToken.None);
        if (result)
            await ToastService.Success("更新", $"更新成功");
        else
            await ToastService.Error("更新", $"更新失败");
    }

    private async Task BackupApplicationAsync()
    {
        var buffer = await HubContext.Clients.Client(ClientId)
            .InvokeAsync<ReadOnlyMemory<byte>>(nameof(IMonitoringClientHub.GetApplicationBackup),
                CancellationToken.None);
        using var streamRef = new DotNetStreamReference(stream: new MemoryStream(buffer.ToArray()));
        await JS.InvokeVoidAsync("downloadFileFromStream", $"{Application.Name}-{DateTime.Now:yyyyMMddHHmmss}",
            streamRef);
    }

    private async Task DownloadLogsAsync()
    {
        var option = new SearchDialogOption<AppLogSearchItem>()
        {
            Title = "日志下载",
            ResetButtonText = "取消",
            QueryButtonText = "下载",
            Model = new AppLogSearchItem(),
            ItemsPerRow = 2,
            RowType = RowType.Inline,
            OnCloseAsync = () => { return Task.CompletedTask; },
            OnResetSearchClick = () => { return Task.CompletedTask; },
            OnSearchClick = () => { return Task.CompletedTask; }
        };

        await DialogService.ShowSearchDialog(option);

        var buffer = await HubContext.Clients.Client(ClientId)
            .InvokeAsync<ReadOnlyMemory<byte>>(nameof(IMonitoringClientHub.GetApplicationLogs), CancellationToken.None);
        using var streamRef = new DotNetStreamReference(stream: new MemoryStream(buffer.ToArray()));
        await JS.InvokeVoidAsync("downloadFileFromStream", $"{Application.Name}-{DateTime.Now:yyyyMMddHHmmss}",
            streamRef);
    }
}