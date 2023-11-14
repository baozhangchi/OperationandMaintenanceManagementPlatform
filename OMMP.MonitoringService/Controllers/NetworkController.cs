using Microsoft.AspNetCore.Mvc;
using OMMP.Common;
using OMMP.Models;
using SqlSugar;

namespace OMMP.MonitoringService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NetworkController : ControllerBase
{
    private readonly LogRepository<NetworkRateLog> _networkRateLogRepository;

    public NetworkController(LogRepository<NetworkRateLog> networkRateLogRepository)
    {
        _networkRateLogRepository = networkRateLogRepository;
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
                await _networkRateLogRepository.GetListAsync(x => x.NetworkCard == networkCardName, count));
        }

        return items;
    }
}