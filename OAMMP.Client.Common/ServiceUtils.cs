#region

using System.Text;
using Newtonsoft.Json;
using OAMMP.Common;
using OAMMP.Models;

#endregion

namespace OAMMP.Client.Common;

public class ServiceUtils
{
	private string? _baseUrl;

	public string? _token;

	private ServiceUtils()
	{
	}

	public static ServiceUtils Instance { get; } = new();

	public void Init(string baseUrl)
	{
		_baseUrl = baseUrl;
	}

	public Task<List<ApplicationItem>?> GetApplications()
	{
		return Send<List<ApplicationItem>>("/api/Application");
	}

	public Task<List<ApplicationLog>?> GetApplicationLogs(long applicationId,
		QueryLogArgs queryLogArgs)
	{
		return Send<List<ApplicationLog>>($"/api/Application/{applicationId}", queryLogArgs, HttpMethod.Post);
	}

	public Task<bool> SaveApplication(ApplicationItem application)
	{
		return Send<bool>("/api/Application", application, HttpMethod.Post);
	}

	public Task<bool> GetApplicationAlive(long applicationId)
	{
		return Send<bool>($"/api/Application/alive/{applicationId}");
	}

	public Task<ApplicationItem?> GetApplication(long applicationId)
	{
		return Send<ApplicationItem>($"/api/Application/app/{applicationId}");
	}

	public Task<bool> DeleteApplications(List<long> applicationIds)
	{
		return Send<bool>("/api/Application", applicationIds, HttpMethod.Delete);
	}

	public Task<List<CpuLog>?> GetCpuLogs(QueryLogArgs args)
	{
		return Send<List<CpuLog>>("/api/Resources/cpu", args, HttpMethod.Post);
	}

	public Task<List<MemoryLog>?> GetMemoryLogs(QueryLogArgs args)
	{
		return Send<List<MemoryLog>>("/api/Resources/memory", args, HttpMethod.Post);
	}

	public Task<List<NetworkLog>?> GetNetworkLogs(QueryLogArgs args)
	{
		return Send<List<NetworkLog>>("/api/Resources/net", args, HttpMethod.Post);
	}

	public Task<List<NetworkLog>?> GetNetworkLogs(string mac, QueryLogArgs args)
	{
		return Send<List<NetworkLog>>($"/api/Resources/net/{mac}", args, HttpMethod.Post);
	}

	public Task<List<ServerResourceLog>?> GetServerResourceLogs(QueryLogArgs args)
	{
		return Send<List<ServerResourceLog>>("/api/Resources", args, HttpMethod.Post);
	}

	public Task<PartitionLog?> GetPartitionLogs(string drive)
	{
		return Send<PartitionLog>($"/api/Resources/partition/{drive}");
	}

	public Task<List<string>?> GetPartitions()
	{
		return Send<List<string>>("/api/Resources/partitions");
	}

	public Task<Dictionary<string, string>?> GetNetworkCards()
	{
		return Send<Dictionary<string, string>>("/api/Resources/net");
	}

	private async Task<T?> Send<T>(string url, object? data = null, HttpMethod? method = null)
	{
		if (string.IsNullOrWhiteSpace(_baseUrl))
		{
			return default;
		}

		var requestMessage = new HttpRequestMessage();
		requestMessage.RequestUri = new Uri($"{_baseUrl!.TrimEnd('/')}/{url.TrimStart('/')}");
		requestMessage.Method = method ?? HttpMethod.Get;
		if (data != null)
			requestMessage.Content =
				new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

		using (var client = new HttpClient())
		{
			var responseMessage = await client.SendAsync(requestMessage);
			var responseContent = await responseMessage.Content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<T>(responseContent);
		}
	}
}