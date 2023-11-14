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
        private readonly LogRepository<MemoryLog> _memoryLogRepository;

        public MemoryController(LogRepository<MemoryLog> memoryLogRepository)
        {
            _memoryLogRepository = memoryLogRepository;
        }

        [HttpGet("{count}")]
        public async Task<IEnumerable<MemoryLog>> GetMemoryData(int count)
        {
            var items = await _memoryLogRepository.GetListAsync(count);
            return items;
        }
    }
}