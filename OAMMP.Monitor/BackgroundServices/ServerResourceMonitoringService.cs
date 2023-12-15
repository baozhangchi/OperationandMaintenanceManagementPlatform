using System.Diagnostics;
using System.Net.Sockets;
using CZGL.SystemInfo;
using Microsoft.Extensions.DependencyInjection;
using OAMMP.Common;
using OAMMP.Models;

namespace OAMMP.Monitor.BackgroundServices;

public class ServerResourceMonitoringService : TimeBackgroundService
{
	private static readonly Dictionary<string, Rate> NetworkRates = new();
	private readonly IServiceProvider _serviceProvider;
	private CPUTime _oldCpuTime;

	public ServerResourceMonitoringService(IServiceProvider serviceProvider)
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
		var random = new Random();
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
			foreach (var networkInfo in NetworkInfo.GetNetworkInfos().Where(x => x.IsSupportIpv4 && !string.IsNullOrWhiteSpace(x.Mac) && x.Status == System.Net.NetworkInformation.OperationalStatus.Up))
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
			foreach (var driveInfo in DriveInfo.GetDrives())
			{
				await repository.InsertAsync(new PartitionLog
				{
					Time = currentTime,
					Free = driveInfo.TotalFreeSpace,
					Name = driveInfo.Name,
					Total = driveInfo.TotalSize,
					Used = driveInfo.TotalSize - driveInfo.TotalFreeSpace
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
		//var client = RepositoryBase.GetClient(GlobalCache.DataSource);

		//var repository = LogRepository<NetworkLog>.CreateInstance(client);
		//var ipAddress = HardwareHelper.GetIpAddresses();
		//await repository.InsertRangeAsync(ipAddress.Select(x => new NetworkLog()
		//{
		//    Mac = x.Item1,
		//    IpAddress = x.Item2,
		//    Time = currentTime
		//}).ToList());

		//var serverResourceRepository = LogRepository<ServerResourceLog>.CreateInstance(client);
		//await serverResourceRepository.InsertAsync(new ServerResourceLog()
		//{
		//    ProcessCount = Process.GetProcesses().Length,
		//    Time = currentTime
		//});

		//var networkRepository = LogRepository<NetworkRateLog>.CreateInstance(client);
		//foreach (var (networkCardName, down, up) in HardwareHelper.GetNetworkRates())
		//{
		//    await networkRepository.InsertAsync(new NetworkRateLog()
		//    {
		//        NetworkCard = networkCardName,
		//        Down = down,
		//        Up = up,
		//        Time = currentTime
		//    });
		//}

		//var memoryRepository = LogRepository<MemoryLog>.CreateInstance(client);
		//var (total, free, used) = HardwareHelper.GetMemoryInfo();
		//await memoryRepository.InsertAsync(new MemoryLog()
		//{
		//    Free = free,
		//    Total = total,
		//    Used = used,
		//    Time = currentTime
		//});

		//var cpuRepository = LogRepository<CpuLog>.CreateInstance(client);
		//await cpuRepository.InsertAsync(new CpuLog()
		//{
		//    Used = HardwareHelper.GetCpuUsed(),
		//    Time = currentTime
		//});

		//var diskRepository = LogRepository<PartitionLog>.CreateInstance(client);
		//var diskInfos = HardwareHelper.GetPartitionInfos();
		//if (diskInfos != null)
		//{
		//    await diskRepository.InsertRangeAsync(diskInfos.Select(x => new PartitionLog()
		//    {
		//        Name = x.Item1,
		//        Total = x.Item2,
		//        Free = x.Item3,
		//        Used = x.Item4,
		//        Time = currentTime
		//    }).ToList());
		//}
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