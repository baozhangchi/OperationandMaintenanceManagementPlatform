using System.Diagnostics;
using OMMP.Common;
using OMMP.Models;

namespace OMMP.MonitoringService.BackgroundServices;

public class ServerResourceMonitoringService : TimeBackgroundService
{
    protected override async Task ExecuteAsync()
    {
        var client = RepositoryBase.GetClient();

        var serverResourceRepository = LogRepository<ServerResourceLog>.CreateInstance(client);
        await serverResourceRepository.InsertAsync(new ServerResourceLog()
        {
            ProcessCount = Process.GetProcesses().Length
        });

        var networkRepository = LogRepository<NetworkRateLog>.CreateInstance(client);
        foreach (var (networkCardName, down, up) in HardwareHelper.GetNetworkRates())
        {
            await networkRepository.InsertAsync(new NetworkRateLog()
            {
                NetworkCard = networkCardName,
                Down = down,
                Up = up
            });
        }

        var memoryRepository = LogRepository<MemoryLog>.CreateInstance(client);
        var (total, free, used) = HardwareHelper.GetMemoryInfo();
        await memoryRepository.InsertAsync(new MemoryLog()
        {
            Free = free,
            Total = total,
            Used = used
        });

        var cpuRepository = LogRepository<CpuLog>.CreateInstance(client);
        await cpuRepository.InsertAsync(new CpuLog()
        {
            Used = HardwareHelper.GetCpuUsed()
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
                Used = x.Item4
            }).ToList());
        }
    }
}