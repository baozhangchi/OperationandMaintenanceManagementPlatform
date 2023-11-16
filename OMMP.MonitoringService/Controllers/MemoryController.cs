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
    public class MemoryController : ControllerBase
    {
        private readonly LogRepository<MemoryLog> _repository;

        public MemoryController(LogRepository<MemoryLog> repository)
        {
            _repository = repository;
        }

        [HttpGet("{count}")]
        public async Task<IEnumerable<MemoryLog>> GetMemoryData(int count)
        {
            var items = await _repository.GetLatestListAsync(count);
            return items;
        }

        [HttpGet("{startTime}/{count}")]
        public async Task<IEnumerable<MemoryLog>> GetMemoryData(DateTime startTime,int count)
        {
            return await _repository.GetLatestListAsync(x => x.Time > startTime,count);
        }

        [HttpGet("byTimePeriods/{startTime}/{endTime}")]
        public async Task<IEnumerable<MemoryLog>> GetCpuUsed(DateTime startTime, DateTime endTime)
        {
            return await _repository.GetLatestListAsync(x => x.Time >= startTime && x.Time <= endTime);
        }
    }
}