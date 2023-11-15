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

public sealed partial class NetworkMonitor
{
    private string _clientApiUrl;
    private Timer _timer;

    [Parameter]
    public string ClientApiUrl
    {
        get => _clientApiUrl;
        set
        {
            if (SetField(ref _clientApiUrl, value))
            {
                // LineChart?.Update(ChartAction.Update);
                LineChart.Reload();
            }
        }
    }

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
        _timer = new Timer();
        _timer.Interval = TimeSpan.FromSeconds(5).TotalMilliseconds;
        _timer.Elapsed += async (_, _) => { await LineChart.Update(ChartAction.AddData); };
    }

    private async Task<ChartDataSource> OnInit()
    {
        _timer.Stop();
        var dataSource = new ChartDataSource();
        dataSource.Options.Title = "网络监控";
        // dataSource.Options.LegendLabelsFontSize = 16;
        dataSource.Options.X.Title = "时间";
        dataSource.Options.Y.Title = "速率";
        dataSource.Options.ShowXScales = false;
        // dataSource.Options.XScalesBorderColor = "red";
        // dataSource.Options.YScalesBorderColor = "red";
        //
        // dataSource.Options.XScalesGridColor = "blue";
        // dataSource.Options.XScalesGridTickColor = "blue";
        // dataSource.Options.XScalesGridBorderColor = "blue";
        //
        // dataSource.Options.YScalesGridColor = "blue";
        // dataSource.Options.YScalesGridTickColor = "blue";
        // dataSource.Options.YScalesGridBorderColor = "blue";
        if (string.IsNullOrWhiteSpace(ClientApiUrl))
        {
            return dataSource;
        }

        using (var client = new HttpClient())
        {
            try
            {
                var responseMessage = await client.GetAsync($"{ClientApiUrl}/api/Network/1000");
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<Dictionary<string, List<NetworkRateLog>>>(responseContent)
                    .ToDictionary(x => x.Key, x => x.Value.Select(_ => new
                    {
                        Time = new DateTime(_.Time.Year, _.Time.Month, _.Time.Day, _.Time.Hour, _.Time.Minute,
                            _.Time.Second),
                        _.Down,
                        _.Up
                    }).ToList());
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
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        _timer.Start();
        return dataSource;
    }
}