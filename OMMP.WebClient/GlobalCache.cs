using System.Net.WebSockets;

namespace OMMP.WebClient;

public class GlobalCache
{
    /// <summary>
    /// 监控客户端
    /// </summary>
    public static List<WebSocket> MonitoringClients { get; } = new List<WebSocket>();
}