using Microsoft.AspNetCore.Mvc;
using OMMP.Common;
using OMMP.Models;
using SqlSugar;

namespace OMMP.MonitoringService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NetworkController : ControllerBase
{
    private readonly LogRepository<NetworkRateLog> _repository;

    public NetworkController(LogRepository<NetworkRateLog> repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public IEnumerable<string> GetAllNetworkNames()
    {
        return HardwareHelper.NetworkCardNames;
    }

    [HttpGet("{count}")]
    public async Task<Dictionary<string, List<NetworkRateLog>>> GetNetworkRates(int count)
    {
        var items = new Dictionary<string, List<NetworkRateLog>>();
        foreach (var networkCardName in HardwareHelper.NetworkCardNames)
        {
            items.Add(networkCardName,
                await _repository.GetLatestListAsync(x => x.NetworkCard == networkCardName, count));
        }

        return items;
    }

    [HttpGet("{startTime}/{count}")]
    public async Task<Dictionary<string, List<NetworkRateLog>>> GetNetworkRates(DateTime startTime, int count)
    {
        var items = new Dictionary<string, List<NetworkRateLog>>();
        foreach (var networkCardName in HardwareHelper.NetworkCardNames)
        {
            items.Add(networkCardName,
                await _repository.GetLatestListAsync(x => x.NetworkCard == networkCardName && x.Time > startTime,
                    count));
        }

        return items;
    }

    [HttpGet("byTimePeriods/{startTime}/{endTime}")]
    public async Task<Dictionary<string, List<NetworkRateLog>>> GetCpuUsed(DateTime startTime, DateTime endTime)
    {
        var items = new Dictionary<string, List<NetworkRateLog>>();
        foreach (var networkCardName in HardwareHelper.NetworkCardNames)
        {
            items.Add(networkCardName,
                await _repository.GetLatestListAsync(x =>
                    x.NetworkCard == networkCardName && x.Time >= startTime && x.Time <= endTime));
        }

        return items;
    }
}