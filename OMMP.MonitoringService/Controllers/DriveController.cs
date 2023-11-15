using Microsoft.AspNetCore.Mvc;
using OMMP.Common;
using OMMP.Models;

namespace OMMP.MonitoringService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DriveController : ControllerBase
    {
        private readonly LogRepository<DriveLog> _partitionLogRepository;

        public DriveController(LogRepository<DriveLog> partitionLogRepository)
        {
            _partitionLogRepository = partitionLogRepository;
        }

        [HttpGet("count")]
        public async Task<Dictionary<string, List<DriveLog>>> GetPartitionLogs(int count)
        {
            var data = new Dictionary<string, List<DriveLog>>();
            foreach (var disk in HardwareHelper.Disks)
            {
                data.Add(disk, await _partitionLogRepository.GetListAsync(x => x.Name == disk, count));
            }

            return data;
        }

        [HttpGet("latest")]
        public async Task<List<DriveLog>> GetPartitionLog()
        {
            var data = new List<DriveLog>();
            foreach (var disk in HardwareHelper.Disks)
            {
                data.Add(await _partitionLogRepository.GetLatestAsync(x => x.Name == disk));
            }

            return data;
        }

        [HttpGet("partitions")]
        public async Task<IEnumerable<string>> GetPartitions()
        {
            return await Task.FromResult(HardwareHelper.Disks);
        }
    }
}