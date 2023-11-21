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
    public class MemoryController : LogController<MemoryLog>
    {
        public MemoryController(LogRepository<MemoryLog> repository) : base(repository)
        {
        }
    }
}