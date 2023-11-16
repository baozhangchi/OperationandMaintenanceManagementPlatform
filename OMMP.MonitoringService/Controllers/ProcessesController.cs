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
    public class ProcessesController : ControllerBase
    {
        private readonly LogRepository<ServerResourceLog> _repository;

        public ProcessesController(LogRepository<ServerResourceLog> repository)
        {
            _repository = repository;
        }

        [HttpGet("{count}")]
        public async Task<List<ServerResourceLog>> GetServerResourceLogData(int count)
        {
            return await _repository.GetLatestListAsync(count);
        }

        [HttpGet("{startTime}/{count}")]
        public async Task<List<ServerResourceLog>> GetServerResourceLogData(DateTime startTime, int count)
        {
            return await _repository.GetLatestListAsync(x => x.Time > startTime, count);
        }

        [HttpGet("byTimePeriods/{startTime}/{endTime}")]
        public async Task<IEnumerable<ServerResourceLog>> GetCpuUsed(DateTime startTime, DateTime endTime)
        {
            return await _repository.GetLatestListAsync(x => x.Time >= startTime && x.Time <= endTime);
        }
    }
}