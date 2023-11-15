using Microsoft.AspNetCore.SignalR.Client;

namespace OMMP.MonitoringService;

public class WebSocketClient
{
    private bool _connected;
    private HubConnection _connection;
    private Uri _uri;

    private WebSocketClient()
    {
    }

    public async void Connect(Uri uri)
    {
        _uri = uri;
        await ConnectAsync();
    }

    private async Task ConnectAsync()
    {
        while (!_connected)
        {
            try
            {
                _connection = new HubConnectionBuilder()
                    .WithUrl(_uri)
                    .Build();
                await _connection.StartAsync();
                Console.WriteLine("连接成功");
                _connected = true;
                _connection.Closed += ReConnect;
                await _connection.InvokeAsync("RegisterClient", Url);
            }
            catch (HttpRequestException httpRequestException)
            {
                _connected = false;
            }
            catch (System.Net.WebSockets.WebSocketException webSocketException)
            {
                _connected = false;
                await ConnectAsync();
            }
        }
    }

    private async Task ReConnect(Exception arg)
    {
        Console.WriteLine("断开连接");
        _connection.Closed -= ReConnect;
        _connected = false;
        await ConnectAsync();
    }

    public static WebSocketClient Instance { get; } = new();
    public string Url { get; set; }
}

public static class WebSocketClientExtensions
{
    public static WebApplication UseWebSocketClient(this WebApplication app)
    {
        var config = app.Services.GetService<IConfiguration>();
        var serviceUri = config["ServiceUri"];
        WebSocketClient.Instance.Url = app.Urls.First();
        WebSocketClient.Instance.Connect(new Uri(serviceUri));
        return app;
    }
}