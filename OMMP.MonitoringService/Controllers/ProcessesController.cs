using Microsoft.AspNetCore.Mvc;
using OMMP.Models;

namespace OMMP.MonitoringService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProcessesController : LogController<ServerResourceLog>
    {
        public ProcessesController(LogRepository<ServerResourceLog> repository) : base(repository)
        {
        }
    }
}