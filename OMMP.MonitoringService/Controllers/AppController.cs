using Microsoft.AspNetCore.Mvc;
using OMMP.Models;

namespace OMMP.MonitoringService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppController : ControllerBase
    {
        private readonly Repository<ApplicationInfo> _applicationRepository;

        public AppController(Repository<ApplicationInfo> applicationRepository)
        {
            _applicationRepository = applicationRepository;
        }

        /// <summary>
        /// 获取服务器上所有应用
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IEnumerable<ApplicationInfo>> GetAllApplications()
        {
            return await _applicationRepository.GetListAsync();
        }

        /// <summary>
        /// 保存应用
        /// 添加或修改应用
        /// </summary>
        /// <param name="application">应用信息</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<bool> SaveApplication([FromBody] ApplicationInfo application)
        {
            return await _applicationRepository.InsertOrUpdateAsync(application);
        }

        /// <summary>
        /// 移除应用
        /// </summary>
        /// <param name="uuid">应用ID</param>
        /// <returns></returns>
        [HttpDelete("{uuid}")]
        public async Task<bool> RemoveApplication(long uuid)
        {
            return await _applicationRepository.DeleteByIdAsync(uuid);
        }

        // /// <summary>
        // /// 获取所有进程线程信息
        // /// </summary>
        // /// <returns></returns>
        // public IEnumerable<(string, int)> GetAllThreadCount()
        // {
        //     
        // }
    }
}