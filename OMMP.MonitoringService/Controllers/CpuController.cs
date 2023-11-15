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
        private readonly LogRepository<CpuLog> _cpuLogRepository;

        public CpuController(LogRepository<CpuLog> cpuLogRepository)
        {
            _cpuLogRepository = cpuLogRepository;
        }

        [HttpGet("{count}")]
        public async Task<IEnumerable<CpuLog>> GetCpuUsed(int count)
        {
            return await _cpuLogRepository.GetListAsync(count);
        }
    }
}