using System.Net.WebSockets;

namespace OMMP.MonitoringService;

internal static class GlobalCache
{
    static GlobalCache()
    {
        Client = new ClientWebSocket();
        HasConnected = false;
        Task.Run(ConnectServer);
    }

    private static void ConnectServer()
    {
        
    }

    public static bool HasConnected { get; private set; }

    public static ClientWebSocket Client { get; }
    public static string DataFolder { get; set; }
}