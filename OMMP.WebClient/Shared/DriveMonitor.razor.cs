using System.Runtime.CompilerServices;
using System.Web;
using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using OMMP.Common;
using OMMP.Models;
using Console = System.Console;

namespace OMMP.WebClient.Shared;

public partial class DriveMonitor : IMonitorComponent
{
    private string _drive;
    private bool _refreshDataSignaler;

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
        base.OnInitialized();
        GlobalCache.Instance.TimerElapsed -= OnTimerElapsed;
        GlobalCache.Instance.TimerElapsed += OnTimerElapsed;
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

        var url = $"api/Drive/latest/{Drive.ToBase64()}";
        var data = await HttpHelper.GetAsync<DriveLog>(url);

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
}