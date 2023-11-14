using System.Diagnostics;
using OMMP.Common;
using OMMP.Models;

namespace OMMP.MonitoringService.BackgroundServices;

public class ApplicationResourceMonitoringService : TimeBackgroundService
{
    protected override async Task ExecuteAsync()
    {
        var client = RepositoryBase.GetClient();
        var applications = await new Repository<ApplicationInfo>(client).GetListAsync();
        var processes = Process.GetProcesses();
        foreach (var application in applications)
        {
            var process = processes.FirstOrDefault(x => x.StartInfo.FileName == application.AppFileName);
            if (process != null)
            {
                var threadCount = process.Threads.Count; //获取线程数
                var cpuUsedResult = Cmder.Run($"ps -C {process.ProcessName} -o %cpu --no-headers");
                var cpuUsed = cpuUsedResult.Split("\n", StringSplitOptions.RemoveEmptyEntries)
                    .Sum(double.Parse);
                var memUsedResult = Cmder.Run($"ps -C {process.ProcessName} -o %mem --no-headers");
                var memUsed = memUsedResult.Split("\n", StringSplitOptions.RemoveEmptyEntries)
                    .Sum(double.Parse);
            }
        }
    }
}