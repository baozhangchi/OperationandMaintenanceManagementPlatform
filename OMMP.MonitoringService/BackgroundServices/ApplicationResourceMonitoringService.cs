using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using CZGL.SystemInfo.Linux;
using Newtonsoft.Json;
using OMMP.Common;
using OMMP.Models;

namespace OMMP.MonitoringService.BackgroundServices;

public class ApplicationResourceMonitoringService : TimeBackgroundService
{
    private static Process _networkMonitorProcess;
    private static readonly List<string> NetworkMonitorResults = new List<string>();

    static ApplicationResourceMonitoringService()
    {
    }

    private static void RunMonitorProcess()
    {
        if (_networkMonitorProcess is { HasExited: false })
        {
            _networkMonitorProcess.Kill(true);
        }

        if (ApplicationSessionIdCollection.Count > 0)
        {
            // Running without root:
            // sudo setcap "cap_net_admin,cap_net_raw,cap_dac_read_search,cap_sys_ptrace+pe" /usr/local/sbin/nethogs
            _networkMonitorProcess = Cmder.Start("nethogs",
                $" -v 2 -d 1 -b -P {string.Join(",", ApplicationSessionIdCollection.Select(x => x.Value))}",
                output =>
                {
                    lock (NetworkMonitorResults)
                    {
                        NetworkMonitorResults.Add(output.Replace("\u001b", " "));
                    }
                });
        }
    }

    private static readonly Dictionary<long, int> ApplicationSessionIdCollection = new Dictionary<long, int>();

    protected override async Task ExecuteAsync(DateTime currentTime)
    {
        var client = RepositoryBase.GetClient(GlobalCache.DataSource);
        var applications = await new Repository<ApplicationInfo>(client).GetListAsync();
        var repository = LogRepository<ApplicationLog>.CreateInstance(client);
        List<string> networkMonitorResults = null;
        lock (NetworkMonitorResults)
        {
            networkMonitorResults = NetworkMonitorResults.ToList();
            NetworkMonitorResults.Clear();
        }

        var applicationSessionIdCollectionChanged = false;
        foreach (var application in applications)
        {
            var process = Process.GetProcessesByName(application.AppFileName).FirstOrDefault();
            if (process != null)
            {
                if (ApplicationSessionIdCollection.ContainsKey(application.UUID))
                {
                    if (ApplicationSessionIdCollection[application.UUID] != process.Id)
                    {
                        ApplicationSessionIdCollection[application.UUID] = process.Id;
                        applicationSessionIdCollectionChanged = true;
                    }
                }
                else
                {
                    ApplicationSessionIdCollection.Add(application.UUID, process.Id);
                    applicationSessionIdCollectionChanged = true;
                }

                var result = Cmder.Run($"ps -p {process.Id} -o %cpu,%mem,rbytes,wbytes --no-headers")
                    .Split(' ', '\n', StringSplitOptions.RemoveEmptyEntries);

                var networkMonitorResult = networkMonitorResults.LastOrDefault(x =>
                    x.Contains(process.Id.ToString()) && x.Contains(process.ProcessName));

                if (!string.IsNullOrWhiteSpace(networkMonitorResult))
                {
                    networkMonitorResult =
                        networkMonitorResult.Substring(networkMonitorResult.IndexOf(process.Id.ToString()));
                    networkMonitorResult = Regex.Replace(networkMonitorResult, @"\[\d;\d{1,3}H", "");
                    var items = networkMonitorResult.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    // Console.WriteLine(networkMonitorResult);
                }

                await repository.InsertAsync(new ApplicationLog()
                {
                    ApplicationId = application.UUID,
                    ThreadCount = process.Threads.Count,
                    CpuUsed = double.Parse(result[0]),
                    MemoryUsed = double.Parse(result[1]),
                    IOReadRate = int.Parse(result[2]),
                    IOWriteRate = int.Parse(result[3]),
                    Time = currentTime
                });
            }
        }

        // if (applicationSessionIdCollectionChanged)
        // {
        //     RunMonitorProcess();
        // }
    }
}