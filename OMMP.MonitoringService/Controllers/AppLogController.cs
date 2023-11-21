using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OMMP.Models;

namespace OMMP.MonitoringService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppLogController : ControllerBase
    {
        private readonly LogRepository<ApplicationLog> _repository;

        public AppLogController(LogRepository<ApplicationLog> repository)
        {
            _repository = repository;
        }

        [HttpGet("{appId}/{count}")]
        public async Task<List<ApplicationLog>> GetData(long appId, int count)
        {
            return await _repository.GetLatestListAsync(x => x.ApplicationId == appId, count);
        }

        [HttpGet("{appId}/{startTime}/{count}")]
        public async Task<List<ApplicationLog>> GetData(long appId, DateTime startTime, int count)
        {
            return await _repository.GetLatestListAsync(x => x.ApplicationId == appId && x.Time > startTime, count);
        }

        [HttpGet("byTimePeriods/{appId}/{startTime}/{endTime}")]
        public async Task<IEnumerable<ApplicationLog>> GetData(long appId, DateTime startTime, DateTime endTime)
        {
            return await _repository.GetLatestListAsync(x =>
                x.ApplicationId == appId && x.Time >= startTime && x.Time <= endTime);
        }
    }
}