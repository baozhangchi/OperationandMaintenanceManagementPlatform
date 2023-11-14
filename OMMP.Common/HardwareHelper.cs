using System.Net.NetworkInformation;
using System.Net.Sockets;
using CZGL.SystemInfo;

namespace OMMP.Common;

public static class HardwareHelper
{
    #region Network

    private static readonly Dictionary<string, Rate> NetworkRates = new Dictionary<string, Rate>();
    private static List<string>? _networkCardNames;
    private static List<NetworkInfo>? _networkInfos;
    private static CPUTime _oldTime;
    private static List<string> _disks;

    /// <summary>
    /// 所有网卡信息
    /// </summary>
    public static List<string> NetworkCardNames
    {
        get
        {
            return _networkCardNames ??= NetworkInfos
                .Select(x => x.Name).ToList();
        }
    }

    public static List<NetworkInfo> NetworkInfos
    {
        get
        {
            return _networkInfos ?? (_networkInfos = NetworkInfo.GetNetworkInfos()
                .Where(x => x.NetworkType == NetworkInterfaceType.Ethernet && x.UnicastAddresses.Any() &&
                            x.UnicastAddresses.All(_ =>
                                _.AddressFamily is AddressFamily.InterNetwork or AddressFamily.InterNetworkV6))
                .ToList());
        }
    }

    /// <summary>
    /// 获取网卡上传下载速度
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<(string, long, long)> GetNetworkRates()
    {
        return NetworkInfos.Select(networkInfo =>
        {
            if (!networkInfo.UnicastAddresses.Any())
            {
                return (networkInfo.Name, 0, 0);
            }

            var newRate = networkInfo.GetIpv4Speed();
            if (!NetworkRates.TryGetValue(networkInfo.Name, out var oldRate))
            {
                oldRate = default;
            }

            var (received, sent) = NetworkInfo.GetSpeed(oldRate, newRate);
            NetworkRates[networkInfo.Name] = newRate;
            return (networkInfo.Name, received.ByteLength, sent.ByteLength);
        });
    }

    /// <summary>
    /// 获取所有Ip地址
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<(string, string)> GetIpAddresses()
    {
        return NetworkInfos.Select(x =>
        {
            return (x.Name,
                x.UnicastAddresses.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork).ToString());
        });
    }

    #endregion

    /// <summary>
    /// 获取所有分区信息
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<(string, long, long, long)> GetPartitionInfos()
    {
        return DiskInfo.GetRealDisk().Select(x => (x.Name, x.TotalSize, x.FreeSpace, x.UsedSize));
    }

    public static List<string> Disks
    {
        get
        {
            if (_disks == null)
            {
                _disks = DiskInfo.GetRealDisk().Select(x => x.Name).ToList();
            }

            return _disks;
        }
    }

    /// <summary>
    /// 获取所有内存信息
    /// </summary>
    /// <returns></returns>
    public static (ulong, ulong, ulong) GetMemoryInfo()
    {
        var memoryValue = MemoryHelper.GetMemoryValue();
        return (memoryValue.TotalPhysicalMemory, memoryValue.AvailablePhysicalMemory, memoryValue.UsedPhysicalMemory);
    }

    /// <summary>
    /// 获取指定线程的进程数
    /// </summary>
    /// <param name="pid"></param>
    /// <returns></returns>
    public static int GetTheNumberOfThreadProcesses(int pid)
    {
        var result = Cmder.Run($"ps -T -p {pid}|wc -l");
        return int.Parse(result);
    }

    /// <summary>
    /// 获取CPU使用率
    /// </summary>
    /// <returns></returns>
    public static double GetCpuUsed()
    {
        var time = CPUHelper.GetCPUTime();
        var used = CPUHelper.CalculateCPULoad(_oldTime, time);
        _oldTime = time;
        return used;
    }
}