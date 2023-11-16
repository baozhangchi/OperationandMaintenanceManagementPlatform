using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OMMP.Models;
using SqlSugar;

namespace OMMP.MonitoringService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CpuController : ControllerBase
    {
        private readonly LogRepository<CpuLog> _repository;

        public CpuController(LogRepository<CpuLog> repository)
        {
            _repository = repository;
        }

        [HttpGet("{count}")]
        public async Task<IEnumerable<CpuLog>> GetCpuUsed(int count)
        {
            return await _repository.GetLatestListAsync(count);
        }

        [HttpGet("{startTime}/{count}")]
        public async Task<IEnumerable<CpuLog>> GetCpuUsed(DateTime startTime, int count)
        {
            return await _repository.GetLatestListAsync(x => x.Time > startTime, count);
        }

        [HttpGet("byTimePeriods/{startTime}/{endTime}")]
        public async Task<IEnumerable<CpuLog>> GetCpuUsed(DateTime startTime, DateTime endTime)
        {
            return await _repository.GetLatestListAsync(x => x.Time >= startTime && x.Time <= endTime);
        }
    }
}