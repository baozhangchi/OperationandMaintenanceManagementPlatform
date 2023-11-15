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

public partial class MemoryMonitor
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
                LineChart?.Reload();
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
        dataSource.Options.Title = "内存使用率";
        // dataSource.Options.LegendLabelsFontSize = 16;
        dataSource.Options.X.Title = "时间";
        dataSource.Options.Y.Title = "使用率";
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
                var responseMessage = await client.GetAsync($"{ClientApiUrl}/api/Memory/1000");
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<List<MemoryLog>>(responseContent);
                dataSource.Labels = data.Select(x => x.Time.ToString("yyyy-MM-dd HH:mm:ss")).ToList();

                dataSource.Data.Add(new ChartDataset()
                {
                    ShowPointStyle = false,
                    PointRadius = 1,
                    PointStyle = ChartPointStyle.Circle,
                    // PointHoverRadius = 10,
                    Tension = 0,
                    BorderWidth = 1,
                    Label = $"Memory",
                    Data = data.Select(x => (object)((double)x.Used / (double)x.Total * 100))
                });
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