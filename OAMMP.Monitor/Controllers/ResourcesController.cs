using System.Net.NetworkInformation;
using CZGL.SystemInfo;
using Microsoft.AspNetCore.Mvc;
using OAMMP.Common;
using OAMMP.Models;
using SqlSugar;

namespace OAMMP.Monitor.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ResourcesController : ControllerBase
{
    private readonly IServiceProvider _provider;

    public ResourcesController(IServiceProvider provider)
    {
        _provider = provider;
    }

    [HttpPost("cpu")]
    public Task<List<CpuLog>> GetCpuLogs([FromBody] QueryLogArgs args)
    {
        return _provider.CreateAsyncScope().ServiceProvider.GetRequiredService<LogRepository<CpuLog>>()
            .GetLogData(args);
    }

    [HttpPost("memory")]
    public Task<List<MemoryLog>> GetMemoryLogs([FromBody] QueryLogArgs args)
    {
        return _provider.CreateAsyncScope().ServiceProvider.GetRequiredService<LogRepository<MemoryLog>>()
            .GetLogData(args);
    }

    [HttpPost("net")]
    public async Task<List<NetworkLog>> GetNetworkLogs([FromBody] QueryLogArgs args)
    {
        using (var repository =
               _provider.CreateAsyncScope().ServiceProvider.GetRequiredService<LogRepository<NetworkLog>>())
        {
            var data = new List<NetworkLog>();
            foreach (var networkInfo in NetworkInfo.GetNetworkInfos())
            {
                var mac = networkInfo.Mac;
                var expression = new Expressionable<NetworkLog>();
                expression.And(x => x.Mac == mac);
                expression.AndIF(args.StartTime.HasValue, x => x.Time > args.StartTime!.Value);
                expression.AndIF(args.EndTime.HasValue, x => x.Time <= args.EndTime!.Value);
                if (args.Count.HasValue)
                {
                    var items = await repository.GetLatestListAsync(expression.ToExpression(), args.Count.Value);
                    data.AddRange(items);
                }
                else
                {
                    var items = await repository.GetLatestListAsync(expression.ToExpression());
                    data.AddRange(items);
                }
            }

            return data;
        }
    }

    [HttpPost("net/{mac}")]
    public Task<List<NetworkLog>> GetNetworkLogs(string mac, [FromBody] QueryLogArgs args)
    {
        return _provider.CreateAsyncScope().ServiceProvider.GetRequiredService<LogRepository<NetworkLog>>()
            .GetLogData(args, x => x.Mac == mac);
    }

    [HttpGet("net")]
    public Task<Dictionary<string, string>> GetNetworkCards()
    {
        return Task.FromResult(NetworkInfo.GetNetworkInfos()
            .Where(x => x.IsSupportIpv4 && !string.IsNullOrWhiteSpace(x.Mac) &&
                        x.Status == OperationalStatus.Up)
            .ToDictionary(x => x.Mac, x => x.Name));
    }

    [HttpPost]
    public Task<List<ServerResourceLog>> GetServerResourceLogs([FromBody] QueryLogArgs args)
    {
        return _provider.CreateAsyncScope().ServiceProvider.GetRequiredService<LogRepository<ServerResourceLog>>()
            .GetLogData(args);
    }

    [HttpGet("partition/{drive}")]
    public async Task<PartitionLog> GetPartitionLog(string drive)
    {
        using (var repository =
               _provider.CreateAsyncScope().ServiceProvider.GetRequiredService<LogRepository<PartitionLog>>())
        {
            return await repository.GetLatestAsync(x => x.Name == drive.FromBase64());
        }
    }

    [HttpGet("partitions")]
    public Task<List<string>> GetPartitions()
    {
        return Task.FromResult(DriveInfo.GetDrives().Select(drive => drive.Name).ToList());
    }
}