using System.Net.WebSockets;

namespace OMMP.WebService;

public class GlobalCache
{
    /// <summary>
    /// 查询网卡名称
    /// </summary>
    public const string QUERY_NETWORK_CARD_NAME = "ls /sys/class/net/ | grep -v \"`ls /sys/devices/virtual/net/`\"";

    /// <summary>
    /// 监控客户端
    /// </summary>
    public static List<WebSocket> MonitoringClients { get; } = new List<WebSocket>();
}