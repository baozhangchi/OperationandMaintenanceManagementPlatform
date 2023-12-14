using System.Collections;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using OAMMP.Client.Common;

namespace OAMMP.Server.Hubs;

public interface IMonitorState : IEnumerable<MonitorServer>
{
	string this[string ip] { get; set; }

	IHubCallerClients? Clients { get; set; }

	void CompleteTask(string taskId, JsonElement data);

	void Remove(string ip);
	Task<T?> InvokeAsync<T>(string clientId, string methodName);
	Task<T?> InvokeAsync<T>(string clientId, string methodName, object? arg1);
	Task<T?> InvokeAsync<T>(string clientId, string methodName, object? arg1, object? arg2);
	Task<T?> InvokeAsync<T>(string clientId, string methodName, object? arg1, object? arg2, object? arg3);
	Task<T?> InvokeAsync<T>(string clientId, string methodName, object? arg1, object? arg2, object? arg3, object? arg4);

	Task<T?> InvokeAsync<T>(string clientId, string methodName, object? arg1, object? arg2, object? arg3, object? arg4,
		object? arg5);

	Task<T?> InvokeAsync<T>(string clientId, string methodName, object? arg1, object? arg2, object? arg3, object? arg4,
		object? arg5, object? arg6);

	Task<T?> InvokeAsync<T>(string clientId, string methodName, object? arg1, object? arg2, object? arg3, object? arg4,
		object? arg5, object? arg6, object? arg7);

	Task<T?> InvokeAsync<T>(string clientId, string methodName, object? arg1, object? arg2, object? arg3, object? arg4,
		object? arg5, object? arg6, object? arg7, object? arg8);

	Task<T?> InvokeAsync<T>(string clientId, string methodName, object? arg1, object? arg2, object? arg3, object? arg4,
		object? arg5, object? arg6, object? arg7, object? arg8, object? arg9);
}

public class SignalRData
{
	public bool IsComplete { get; set; }

	public object? Data { get; set; }
	public Type? DataType { get; set; }
}

public class MonitorState : IMonitorState
{
	public IHubCallerClients? Clients { get; set; }

	public IEnumerator<MonitorServer> GetEnumerator()
	{
		return _data.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)_data).GetEnumerator();
	}

	private readonly List<MonitorServer> _data = new();

	public string this[string ip]
	{
		get => _data.SingleOrDefault(x => x.Ip == ip)?.HubClientId ?? string.Empty;

		set
		{
			var item = _data.SingleOrDefault(x => x.Ip == ip);
			if (item != null)
			{
				item.HubClientId = value;
			}
			else
			{
				_data.Add(new MonitorServer
				{
					Ip = ip,
					HubClientId = value,
					Label = ip
				});
			}
		}
	}

	private readonly Dictionary<string, SignalRData> _taskHash = new();

	public void CompleteTask(string taskId, JsonElement data)
	{
		if (_taskHash.TryGetValue(taskId, out var item))
		{
			item.IsComplete = true;
			item.Data = data.Deserialize(item.DataType ?? typeof(string));
		}
	}

	public void Remove(string ip)
	{
		_data.RemoveAll(x => x.Ip == ip);
	}

	public async Task<T?> InvokeAsync<T>(string clientId, string methodName)
	{
		RegisterTask<T>(out var taskId);
		await Clients?.Client(clientId).SendAsync(methodName, taskId, CancellationToken.None)!;
		return await WaitTaskComplete<T>(taskId);
	}

	private void RegisterTask<T>(out string taskId)
	{
		taskId = Guid.NewGuid().ToString("N");
		var data = new SignalRData();
		data.DataType = typeof(T);
		_taskHash.Add(taskId, data);
	}

	private async Task<T?> WaitTaskComplete<T>(string taskId)
	{
		if (_taskHash.TryGetValue(taskId, out var item))
		{
			while (!item.IsComplete)
			{
				await Task.Delay(10);
			}

			_taskHash.Remove(taskId);

			if (item.Data is T result)
			{
				return result;
			}

			return default;
		}

		return default;
	}

	public async Task<T?> InvokeAsync<T>(string clientId, string methodName, object? arg1)
	{
		RegisterTask<T>(out var taskId);
		await Clients?.Client(clientId).SendAsync(methodName, taskId, arg1, CancellationToken.None)!;
		return await WaitTaskComplete<T>(taskId);
	}

	public async Task<T?> InvokeAsync<T>(string clientId, string methodName, object? arg1, object? arg2)
	{
		RegisterTask<T>(out var taskId);
		await Clients?.Client(clientId).SendAsync(methodName, taskId, arg1, arg2, CancellationToken.None)!;
		return await WaitTaskComplete<T>(taskId);
	}

	public async Task<T?> InvokeAsync<T>(string clientId, string methodName, object? arg1, object? arg2, object? arg3)
	{
		RegisterTask<T>(out var taskId);
		await Clients?.Client(clientId).SendAsync(methodName, taskId, arg1, arg2, arg3, CancellationToken.None)!;
		return await WaitTaskComplete<T>(taskId);
	}

	public async Task<T?> InvokeAsync<T>(string clientId, string methodName, object? arg1, object? arg2, object? arg3,
		object? arg4)
	{
		RegisterTask<T>(out var taskId);
		await Clients?.Client(clientId).SendAsync(methodName, taskId, arg1, arg2, arg3, arg4, CancellationToken.None)!;
		return await WaitTaskComplete<T>(taskId);
	}

	public async Task<T?> InvokeAsync<T>(string clientId, string methodName, object? arg1, object? arg2, object? arg3,
		object? arg4,
		object? arg5)
	{
		RegisterTask<T>(out var taskId);
		await Clients?.Client(clientId)
			.SendAsync(methodName, taskId, arg1, arg2, arg3, arg4, arg5, CancellationToken.None)!;
		return await WaitTaskComplete<T>(taskId);
	}

	public async Task<T?> InvokeAsync<T>(string clientId, string methodName, object? arg1, object? arg2, object? arg3,
		object? arg4,
		object? arg5, object? arg6)
	{
		RegisterTask<T>(out var taskId);
		await Clients?.Client(clientId)
			.SendAsync(methodName, taskId, arg1, arg2, arg3, arg4, arg5, arg6, CancellationToken.None)!;
		return await WaitTaskComplete<T>(taskId);
	}

	public async Task<T?> InvokeAsync<T>(string clientId, string methodName, object? arg1, object? arg2, object? arg3,
		object? arg4,
		object? arg5, object? arg6, object? arg7)
	{
		RegisterTask<T>(out var taskId);
		await Clients?.Client(clientId)
			.SendAsync(methodName, taskId, arg1, arg2, arg3, arg4, arg5, arg6, arg7, CancellationToken.None)!;
		return await WaitTaskComplete<T>(taskId);
	}

	public async Task<T?> InvokeAsync<T>(string clientId, string methodName, object? arg1, object? arg2, object? arg3,
		object? arg4,
		object? arg5, object? arg6, object? arg7, object? arg8)
	{
		RegisterTask<T>(out var taskId);
		await Clients?.Client(clientId)
			.SendAsync(methodName, taskId, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, CancellationToken.None)!;
		return await WaitTaskComplete<T>(taskId);
	}

	public async Task<T?> InvokeAsync<T>(string clientId, string methodName, object? arg1, object? arg2, object? arg3,
		object? arg4,
		object? arg5, object? arg6, object? arg7, object? arg8, object? arg9)
	{
		RegisterTask<T>(out var taskId);
		await Clients?.Client(clientId)
			.SendAsync(methodName, taskId, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9,
				CancellationToken.None)!;
		return await WaitTaskComplete<T>(taskId);
	}
}