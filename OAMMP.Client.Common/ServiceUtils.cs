#region

using System;
using System.Text;
using Newtonsoft.Json;
using OAMMP.Common;
using OAMMP.Models;
using static System.Net.Mime.MediaTypeNames;

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

	public Task<List<ApplicationItem>?> GetApplications(string connectionId)
	{
		return Send<List<ApplicationItem>>($"/api/Application/{connectionId}");
	}

	public Task<List<ApplicationLog>?> GetApplicationLogs(string connectionId, long applicationId,
		QueryLogArgs queryLogArgs)
	{
		return Send<List<ApplicationLog>>($"/api/Application/{connectionId}/{applicationId}", queryLogArgs, HttpMethod.Post);
	}

	public Task<bool> SaveApplication(string connectionId, ApplicationItem application)
	{
		return Send<bool>($"/api/Application/{connectionId}", application, HttpMethod.Post);
	}

	public Task<bool> GetApplicationAlive(string connectionId, long applicationId)
	{
		return Send<bool>($"/api/Application/alive/{connectionId}/{applicationId}");
	}

	public Task<ApplicationItem?> GetApplication(string connectionId, long applicationId)
	{
		return Send<ApplicationItem>($"/api/Application/app/{connectionId}/{applicationId}");
	}

	public Task<bool> DeleteApplications(string connectionId, List<long> applicationIds)
	{
		return Send<bool>($"/api/Application/{connectionId}", applicationIds, HttpMethod.Delete);
	}

	public Task<List<CpuLog>?> GetCpuLogs(string connectionId, QueryLogArgs args)
	{
		return Send<List<CpuLog>>($"/api/Resources/cpu/{connectionId}", args, HttpMethod.Post);
	}

	public Task<List<MemoryLog>?> GetMemoryLogs(string connectionId, QueryLogArgs args)
	{
		return Send<List<MemoryLog>>($"/api/Resources/memory/{connectionId}", args, HttpMethod.Post);
	}

	//public Task<List<NetworkLog>?> GetNetworkLogs(string connectionId, QueryLogArgs args)
	//{
	//	return Send<List<NetworkLog>>($"/api/Resources/net/{connectionId}", args, HttpMethod.Post);
	//}

	public Task<List<NetworkLog>?> GetNetworkLogs(string connectionId, string mac, QueryLogArgs args)
	{
		return Send<List<NetworkLog>>($"/api/Resources/net/{connectionId}/{mac}", args, HttpMethod.Post);
	}

	public Task<List<ServerResourceLog>?> GetServerResourceLogs(string connectionId, QueryLogArgs args)
	{
		return Send<List<ServerResourceLog>>($"/api/Resources/{connectionId}", args, HttpMethod.Post);
	}

	public Task<PartitionLog?> GetPartitionLogs(string connectionId, string drive)
	{
		return Send<PartitionLog>($"/api/Resources/partition/{connectionId}/{drive}");
	}

	public Task<List<string>?> GetPartitions(string connectionId)
	{
		return Send<List<string>>($"/api/Resources/partitions/{connectionId}");
	}

	public Task<Dictionary<string, string>?> GetNetworkCards(string connectionId)
	{
		return Send<Dictionary<string, string>>($"/api/Resources/net/{connectionId}");
	}

	private async Task<T?> Send<T>(string url, object? data = null, HttpMethod? method = null)
	{
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