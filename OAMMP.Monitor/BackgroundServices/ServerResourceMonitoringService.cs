using System.Diagnostics;
using System.Net.Sockets;
using CZGL.SystemInfo;
using OAMMP.Common;
using OAMMP.Models;

namespace OAMMP.Monitor.BackgroundServices;

public class ServerResourceMonitoringService : TimeBackgroundService<ServerResourceMonitoringService>
{
    private static readonly Dictionary<string, Rate> NetworkRates = new();
    private readonly IServiceProvider _serviceProvider;
    private CPUTime _oldCpuTime;

    public ServerResourceMonitoringService(IServiceProvider serviceProvider,
        ILogger<ServerResourceMonitoringService> logger) : base(logger)
    {
        _serviceProvider = serviceProvider;
    }

    private double GetCpuUsed()
    {
        var time = CPUHelper.GetCPUTime();
        var used = CPUHelper.CalculateCPULoad(_oldCpuTime, time);
        _oldCpuTime = time;
        return used;
    }

    protected override async Task ExecuteAsync(DateTime currentTime)
    {
        var provider = _serviceProvider.CreateAsyncScope().ServiceProvider;
        using (var repository = provider.GetRequiredService<LogRepository<CpuLog>>())
        {
            await repository.InsertAsync(new CpuLog
            {
                Time = currentTime,
                Used = GetCpuUsed()
            });
        }

        using (var repository = provider.GetRequiredService<LogRepository<MemoryLog>>())
        {
            var memoryValue = MemoryHelper.GetMemoryValue();
            await repository.InsertAsync(new MemoryLog
            {
                Time = currentTime,
                Used = memoryValue.UsedPhysicalMemory,
                Free = memoryValue.AvailablePhysicalMemory,
                Total = memoryValue.TotalPhysicalMemory
            });
        }

        using (var repository = provider.GetRequiredService<LogRepository<NetworkLog>>())
        {
            long up = 0, down = 0;
            foreach (var networkInfo in NetworkInfo.GetNetworkInfos().Where(x =>
                         x.IsSupportIpv4 && !string.IsNullOrWhiteSpace(x.Mac) &&
                         x.Status == System.Net.NetworkInformation.OperationalStatus.Up))
            {
                var rate = GetNetworkRates(networkInfo);
                up += rate.Item1;
                down += rate.Item2;
                await repository.InsertAsync(new NetworkLog
                {
                    Time = currentTime,
                    IpAddress = networkInfo.UnicastAddresses
                        .First(x => x.AddressFamily == AddressFamily.InterNetwork).ToString(),
                    Down = rate.Item1,
                    Up = rate.Item2,
                    Mac = networkInfo.Mac
                });
            }

            await repository.InsertAsync(new NetworkLog
            {
                Time = currentTime,
                IpAddress = "::1",
                Down = down,
                Up = up,
                Mac = Consts.TOTAL_NETWORKS_MAC
            });
        }

        using (var repository = provider.GetRequiredService<LogRepository<PartitionLog>>())
        {
            foreach (var driveInfo in DiskInfo.GetRealDisk())
            {
                await repository.InsertAsync(new PartitionLog
                {
                    Time = currentTime,
                    Free = driveInfo.FreeSpace,
                    Name = driveInfo.Name,
                    Total = driveInfo.TotalSize,
                    Used = driveInfo.UsedSize
                });
            }
        }

        using (var repository = provider.GetRequiredService<LogRepository<ServerResourceLog>>())
        {
            await repository.InsertAsync(new ServerResourceLog
            {
                Time = currentTime,
                ProcessCount = Process.GetProcesses().Length
            });
        }
    }

    public static (long, long) GetNetworkRates(NetworkInfo networkInfo)
    {
        if (!networkInfo.UnicastAddresses.Any())
        {
            return (0, 0);
        }

        var newRate = networkInfo.GetIpv4Speed();
        if (!NetworkRates.TryGetValue(networkInfo.Name, out var oldRate))
        {
            oldRate = default;
        }

        var (received, sent) = NetworkInfo.GetSpeed(oldRate, newRate);
        NetworkRates[networkInfo.Name] = newRate;
        return (received.ByteLength, sent.ByteLength);
    }
}