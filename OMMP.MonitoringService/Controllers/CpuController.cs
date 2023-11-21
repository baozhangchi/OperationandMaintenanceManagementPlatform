using Microsoft.AspNetCore.Mvc;
using OMMP.Models;

namespace OMMP.MonitoringService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CpuController : LogController<CpuLog>
    {
        public CpuController(LogRepository<CpuLog> repository) : base(repository)
        {
        }
    }
}