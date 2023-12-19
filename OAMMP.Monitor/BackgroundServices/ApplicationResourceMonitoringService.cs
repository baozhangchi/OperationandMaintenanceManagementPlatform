using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OAMMP.Common;
using OAMMP.Models;

namespace OAMMP.Monitor.BackgroundServices;

public class ApplicationResourceMonitoringService : TimeBackgroundService<ApplicationResourceMonitoringService>
{
    private static readonly Dictionary<long, int> ApplicationSessionIdCollection = new();

    private readonly Regex _cpuUsageRegex = new(@"process_cpu_usage (?<value>[\d.]+)");

    private readonly Regex _memoryRegex = new(@"jvm_memory_used_bytes\{[\S ]+\} (?<value>[\d.E]+)");

    private readonly Regex _requestCountRegex = new(@"http_server_requests_seconds_count\{[\S ]+\} (?<value>[\d.E]+)");

    private readonly Regex _requestSecondsRegex = new(@"http_server_requests_seconds_sum\{[\S]+\} (?<value>[\d.E]+)");

    private readonly Regex _threadCountRegex = new Regex(@"jvm_threads_live_threads (?<value>[\d.E]+)");

    private readonly IServiceProvider _serviceProvider;

    public ApplicationResourceMonitoringService(IServiceProvider serviceProvider,
        ILogger<ApplicationResourceMonitoringService> logger) : base(logger)
    {
        _serviceProvider = serviceProvider;
    }

    private async Task<bool> CheckAppStatus(ApplicationItem application)
    {
        var url = $"{application.AppUrl?.TrimEnd('/')}/ok";
        using (var client = new HttpClient())
        {
            var content = await client.GetStringAsync(url);
            if (!string.IsNullOrEmpty(content))
            {
                var obj = JsonConvert.DeserializeObject<JObject>(content);
                if (obj != null && obj.TryGetValue("data", out var value))
                {
                    if (value.ToString().Equals("ok"))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    protected override async Task ExecuteAsync(DateTime currentTime)
    {
        var applications = await GetAllApplications();
        using (var repository = _serviceProvider.CreateAsyncScope().ServiceProvider
                   .GetRequiredService<LogRepository<ApplicationLog>>())
        {
            foreach (var application in applications)
            {
                var process = Process.GetProcessesByName(application.AppFileName).FirstOrDefault();
#if !DEBUG
                if (process != null)
                {
#endif
                var log = await GetApplicationLog(application, process);
                log.Time = currentTime;
                await repository.InsertAsync(log);

#if !DEBUG
                }
#endif
            }
        }
    }

    private async Task<List<ApplicationItem>> GetAllApplications()
    {
        using (var repository = _serviceProvider.CreateAsyncScope().ServiceProvider
                   .GetRequiredService<Repository<ApplicationItem>>())
        {
            return await repository.GetListAsync();
        }
    }

    private async Task<ApplicationLog> GetApplicationLog(ApplicationItem application, Process? process)
    {
        var log = new ApplicationLog
        {
            ApplicationId = application.UUID,
            //ThreadCount = process.Threads.Count
        };
        log.IsLive = await CheckAppStatus(application);
        var url = $"{application.AppUrl?.TrimEnd('/')}/actuator/prometheus";
        string? content = null;
        using (var client = new HttpClient())
        {
            content = await client.GetStringAsync(url);
        }

        if (!string.IsNullOrWhiteSpace(content))
        {
            var cpuMatch = _cpuUsageRegex.Match(content);
            if (cpuMatch.Success)
            {
                log.CpuUsage = double.Parse(cpuMatch.Groups["value"].Value);
            }

            var memoryMatch = _memoryRegex.Match(content);
            if (memoryMatch.Success)
            {
                log.MemoryUsage = double.Parse(memoryMatch.Groups["value"].Value);
            }

            var threadCountMatch=_threadCountRegex.Match(content);
            if (threadCountMatch.Success)
            {
                log.ThreadCount = (int)double.Parse(threadCountMatch.Groups["value"].Value);
            }

            var requestSecondsMatches = _requestSecondsRegex.Matches(content);
            var requestSecondsSum = requestSecondsMatches.Select(x => double.Parse(x.Groups["value"].Value)).Sum();
            var requestCountMatches = _requestCountRegex.Matches(content);
            var requestCountSum = requestCountMatches.Select(x => double.Parse(x.Groups["value"].Value)).Sum();
            if (requestCountSum == 0)
            {
                log.AverageResponseTime = 0;
            }
            else
            {
                log.AverageResponseTime = requestSecondsSum / requestCountSum;
            }
        }

        return log;
    }
}