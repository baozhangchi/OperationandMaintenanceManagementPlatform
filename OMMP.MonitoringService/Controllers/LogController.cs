using Microsoft.AspNetCore.Mvc;
using OMMP.Models;

namespace OMMP.MonitoringService.Controllers;

[Route("api/[controller]")]
[ApiController]
public abstract class LogController<T> : ControllerBase
    where T : LogTableBase, new()
{
    protected readonly LogRepository<T> _repository;

    protected LogController(LogRepository<T> repository)
    {
        _repository = repository;
    }

    [HttpGet("{count}")]
    public virtual async Task<List<T>> GetData(int count)
    {
        return await _repository.GetLatestListAsync(count);
    }

    [HttpGet("{startTime}/{count}")]
    public virtual async Task<List<T>> GetData(DateTime startTime, int count)
    {
        return await _repository.GetLatestListAsync(x => x.Time > startTime, count);
    }

    [HttpGet("byTimePeriods/{startTime}/{endTime}")]
    public virtual async Task<IEnumerable<T>> GetData(DateTime startTime, DateTime endTime)
    {
        return await _repository.GetLatestListAsync(x => x.Time >= startTime && x.Time <= endTime);
    }
}