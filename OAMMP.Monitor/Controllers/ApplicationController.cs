using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Client;
using OAMMP.Common;
using OAMMP.Models;

namespace OAMMP.Monitor.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ApplicationController : ControllerBase
{
    private readonly Repository<ApplicationItem> _applicationItemRepository;
    private readonly LogRepository<ApplicationLog> _applicationLogRepository;
    private readonly IServiceProvider _provider;

    public ApplicationController(Repository<ApplicationItem> applicationItemRepository,LogRepository<ApplicationLog> applicationLogRepository,IServiceProvider provider)
    {
        _applicationItemRepository = applicationItemRepository;
        _applicationLogRepository = applicationLogRepository;
        _provider = provider;
    }

    [HttpGet]
    public Task<List<ApplicationItem>> GetApplications()
    {
        return _provider.CreateAsyncScope().ServiceProvider.GetRequiredService<Repository<ApplicationItem>>()
            .GetListAsync();
    }

    [HttpPost]
    public async Task<bool> SaveApplication([FromBody] ApplicationItem item)
    {
        var result = await _applicationItemRepository.InsertOrUpdateAsync(item);
        if (result)
        {
            var client = _provider.CreateAsyncScope().ServiceProvider
                .GetRequiredService<SignalRClient<IMonitorClientHandler>>();
            await client.Connection!.SendAsync("ApplicationsUpdated", await _applicationItemRepository.GetListAsync());
        }

        return result;
    }

    [HttpPost("{applicationId:long}")]
    public Task<List<ApplicationLog>> GetApplicationLogs(long applicationId,
        [FromBody] QueryLogArgs args)
    {
        return _applicationLogRepository.GetLogData(args, x => x.ApplicationId == applicationId);
    }

    [HttpGet("alive/{applicationId:long}")]
    public async Task<bool> GetApplicationAlive(long applicationId)
    {
        var item = await _applicationLogRepository.GetLatestAsync(x => x.ApplicationId == applicationId);
        return item is { IsLive: true };
    }

    [HttpGet("app/{applicationId:long}")]
    public Task<ApplicationItem> GetApplication(long applicationId)
    {
        return _applicationItemRepository.GetByIdAsync(applicationId);
    }

    [HttpDelete]
    public async Task<bool> DeleteApplications([FromBody] List<long> applicationIds)
    {
        var items = await _applicationItemRepository.GetListAsync(x => applicationIds.Contains(x.UUID));
        var result = await _applicationItemRepository.DeleteAsync(items);
        if (result)
        {
            var client = _provider.CreateAsyncScope().ServiceProvider
                .GetRequiredService<SignalRClient<IMonitorClientHandler>>();
            await client.Connection!.SendAsync("ApplicationsUpdated", await _applicationItemRepository.GetListAsync());
        }

        return result;
    }

    [HttpPost("app/update/{applicationId:long}")]
    public async Task<ActionResult> UpdateVersion(long applicationId)
    {
        var files = Request.Form.Files;
        //判断是否有文件上传
        if (files.Count == 0) return NoContent();

        var file = files.First();
        var application = await _applicationItemRepository.GetByIdAsync(applicationId);
        if (application == null) return NoContent();

        var targetFolder = application.AppFolder;
        var tempFile = Path.GetTempFileName();
        using (var temp = System.IO.File.Create(tempFile))
        {
            await file.CopyToAsync(temp);
        }

        // TODO 获取应用线程，如果能获取到，则关闭线程
        await Cmder.RunAndWaitForExit("kill", "-9 "); // TODO 关闭进程 
        await Cmder.RunAndWaitForExit("", $"-o {tempFile} -d {targetFolder}"); //TODO 解压更新包
        Cmder.Run(""); // TODO 启动应用
        System.IO.File.Delete(tempFile);
        return Ok(true);
    }

    [HttpPost("stop/{applicationId:long}")]
    public async Task<ActionResult> StopApplication(long applicationId)
    {
        var application = await _applicationItemRepository.GetByIdAsync(applicationId);
        if (application == null) return base.NoContent();
        // TODO 获取应用线程，如果不能获取到，则返回false,否则停止线程，停止成功则返回true,否则返回false
        await Cmder.RunAndWaitForExit("");
        return Ok(true);
    }

    [HttpPost("start/{applicationId:long}")]
    public async Task<ActionResult> StartApplication(long applicationId)
    {
        var application = await _applicationItemRepository.GetByIdAsync(applicationId);
        if (application == null) return base.NoContent();
        // TODO 获取应用线程，如果能获取到，则返回true,否则按启动命令启动线程，启动成功则返回true,否则返回false
        await Cmder.RunAndWaitForExit("");
        return Ok(true);
    }

    [HttpGet("app/log/{applicationId:long}")]
    public async Task<ActionResult> DownloadLogs(long applicationId)
    {
        var application = await _applicationItemRepository.GetByIdAsync(applicationId);
        if (application == null) return base.NoContent();
        throw new NotImplementedException();
    }

    [HttpDelete("app/log/{applicationId:long}")]
    public async Task<ActionResult> DeleteLogsZip(long applicationId, [FromBody] List<string> files)
    {
        var application = await _applicationItemRepository.GetByIdAsync(applicationId);
        if (application == null) return Ok(false);
        // TODO 删除日志
        return Ok(true);
    }

    [HttpGet("app/backup/{applicationId:long}")]
    public async Task<ActionResult> ApplicationBackup(long applicationId)
    {
        var application = await _applicationItemRepository.GetByIdAsync(applicationId);
        if (application == null) return base.NoContent();

        // TODO 压缩应用目录并返回
        throw new NotImplementedException();
    }
}