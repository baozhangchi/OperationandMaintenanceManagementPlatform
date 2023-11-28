using System.Diagnostics;
using OMMP.Common;
using OMMP.Models;

namespace OMMP.MonitoringService.BackgroundServices;

public class ServerResourceMonitoringService : TimeBackgroundService
{
    protected override async Task ExecuteAsync(DateTime currentTime)
    {
        var client = RepositoryBase.GetClient(GlobalCache.DataSource);

        var repository = LogRepository<NetworkLog>.CreateInstance(client);
        var ipAddress = HardwareHelper.GetIpAddresses();
        await repository.InsertRangeAsync(ipAddress.Select(x => new NetworkLog()
        {
            NetworkCardName = x.Item1,
            IpAddress = x.Item2,
            Time = currentTime
        }).ToList());

        var serverResourceRepository = LogRepository<ServerResourceLog>.CreateInstance(client);
        await serverResourceRepository.InsertAsync(new ServerResourceLog()
        {
            ProcessCount = Process.GetProcesses().Length,
            Time = currentTime
        });

        var networkRepository = LogRepository<NetworkRateLog>.CreateInstance(client);
        foreach (var (networkCardName, down, up) in HardwareHelper.GetNetworkRates())
        {
            await networkRepository.InsertAsync(new NetworkRateLog()
            {
                NetworkCard = networkCardName,
                Down = down,
                Up = up,
                Time = currentTime
            });
        }

        var memoryRepository = LogRepository<MemoryLog>.CreateInstance(client);
        var (total, free, used) = HardwareHelper.GetMemoryInfo();
        await memoryRepository.InsertAsync(new MemoryLog()
        {
            Free = free,
            Total = total,
            Used = used,
            Time = currentTime
        });

        var cpuRepository = LogRepository<CpuLog>.CreateInstance(client);
        await cpuRepository.InsertAsync(new CpuLog()
        {
            Used = HardwareHelper.GetCpuUsed(),
            Time = currentTime
        });

        var diskRepository = LogRepository<DriveLog>.CreateInstance(client);
        var diskInfos = HardwareHelper.GetPartitionInfos();
        if (diskInfos != null)
        {
            await diskRepository.InsertRangeAsync(diskInfos.Select(x => new DriveLog()
            {
                Name = x.Item1,
                Total = x.Item2,
                Free = x.Item3,
                Used = x.Item4,
                Time = currentTime
            }).ToList());
        }
    }
}