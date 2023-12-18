using Microsoft.AspNetCore.Mvc;
using OAMMP.Common;
using OAMMP.Models;

namespace OAMMP.Monitor.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ApplicationController : ControllerBase
{
	private readonly IServiceProvider _provider;

	public ApplicationController(IServiceProvider provider)
	{
		_provider = provider;
	}

	[HttpGet]
	public Task<List<ApplicationItem>> GetApplications()
	{
		return _provider.CreateAsyncScope().ServiceProvider.GetRequiredService<Repository<ApplicationItem>>()
			.GetListAsync();
	}

	[HttpPost]
	public Task<bool> SaveApplication([FromBody] ApplicationItem item)
	{
		return _provider.CreateAsyncScope().ServiceProvider.GetRequiredService<Repository<ApplicationItem>>()
			.InsertOrUpdateAsync(item);
	}

	[HttpPost("{applicationId}")]
	public Task<List<ApplicationLog>> GetApplicationLogs(long applicationId,
		[FromBody] QueryLogArgs args)
	{
		return _provider.CreateAsyncScope().ServiceProvider.GetRequiredService<LogRepository<ApplicationLog>>()
			.GetLogData(args, x => x.ApplicationId == applicationId);
	}

	[HttpGet("alive/{applicationId}")]
	public async Task<bool> GetApplicationAlive(long applicationId)
	{
		var item = await _provider.CreateAsyncScope().ServiceProvider
			.GetRequiredService<LogRepository<ApplicationLog>>()
			.GetLatestAsync(x => x.ApplicationId == applicationId);
		return item?.IsLive ?? false;
	}

	[HttpGet("app/{applicationId}")]
	public Task<ApplicationItem> GetApplication(long applicationId)
	{
		return _provider.CreateAsyncScope().ServiceProvider.GetRequiredService<Repository<ApplicationItem>>()
			.GetByIdAsync(applicationId);
	}

	[HttpDelete]
	public async Task<bool> DeleteApplications([FromBody] List<long> applicationIds)
	{
		using (var repository =
			   _provider.CreateAsyncScope().ServiceProvider.GetRequiredService<Repository<ApplicationItem>>())
		{
			var items = await repository.GetListAsync(x => applicationIds.Contains(x.UUID));
			return await repository.DeleteAsync(items);
		}
	}
}