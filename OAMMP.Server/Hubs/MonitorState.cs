#region

using System.Collections;
using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OAMMP.Client.Common;

#endregion

namespace OAMMP.Server.Hubs;

public interface IMonitorState : IEnumerable<MonitorServer>
{
	string this[string ip] { get; set; }

	IHubCallerClients? Clients { get; set; }

	void CompleteTask(string taskId, JToken data);

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
	private readonly ILogger<IMonitorState> _logger;
	private readonly List<MonitorServer> _data = new();

	private readonly ConcurrentDictionary<string, SignalRData> _taskHash = new();
	public IHubCallerClients? Clients { get; set; }

	public IEnumerator<MonitorServer> GetEnumerator()
	{
		return _data.GetEnumerator();
	}

	public MonitorState(ILogger<IMonitorState> logger)
	{
		_logger = logger;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)_data).GetEnumerator();
	}

	public string this[string ip]
	{
		get => _data.SingleOrDefault(x => x.Ip == ip)?.HubClientId ?? string.Empty;

		set
		{
			var item = _data.SingleOrDefault(x => x.Ip == ip);
			if (item != null)
				item.HubClientId = value;
			else
				_data.Add(new MonitorServer
				{
					Ip = ip,
					HubClientId = value,
					Label = ip
				});
		}
	}

	public void CompleteTask(string taskId, JToken data)
	{
		_logger.LogDebug(data.ToString());
		while (_taskHash.TryGetValue(taskId, out var item))
		{
			item.IsComplete = true;
			try
			{
				item.Data = data.ToObject(item.DataType ?? typeof(string));
			}
			catch
			{
				item.Data = null;
			}
			_logger.LogDebug(JsonConvert.SerializeObject(item.Data));
		}
	}

	public void Remove(string ip)
	{
		_data.RemoveAll(x => x.Ip == ip);
	}

	public async Task<T?> InvokeAsync<T>(string clientId, string methodName)
	{
		
		var taskId = await RegisterTask<T>();
		await Clients?.Client(clientId).SendAsync(methodName, taskId, CancellationToken.None)!;
		return await WaitTaskComplete<T>(taskId);
	}

	public async Task<T?> InvokeAsync<T>(string clientId, string methodName, object? arg1)
	{
		
		var taskId = await RegisterTask<T>();
		await Clients?.Client(clientId).SendAsync(methodName, taskId, arg1, CancellationToken.None)!;
		return await WaitTaskComplete<T>(taskId);
	}

	public async Task<T?> InvokeAsync<T>(string clientId, string methodName, object? arg1, object? arg2)
	{
		
		var taskId = await RegisterTask<T>();
		await Clients?.Client(clientId).SendAsync(methodName, taskId, arg1, arg2, CancellationToken.None)!;
		return await WaitTaskComplete<T>(taskId);
	}

	public async Task<T?> InvokeAsync<T>(string clientId, string methodName, object? arg1, object? arg2, object? arg3)
	{
		
		var taskId = await RegisterTask<T>();
		await Clients?.Client(clientId).SendAsync(methodName, taskId, arg1, arg2, arg3, CancellationToken.None)!;
		return await WaitTaskComplete<T>(taskId);
	}

	public async Task<T?> InvokeAsync<T>(string clientId, string methodName, object? arg1, object? arg2, object? arg3,
		object? arg4)
	{
		
		var taskId = await RegisterTask<T>();
		await Clients?.Client(clientId).SendAsync(methodName, taskId, arg1, arg2, arg3, arg4, CancellationToken.None)!;
		return await WaitTaskComplete<T>(taskId);
	}

	public async Task<T?> InvokeAsync<T>(string clientId, string methodName, object? arg1, object? arg2, object? arg3,
		object? arg4,
		object? arg5)
	{
		
		var taskId = await RegisterTask<T>();
		await Clients?.Client(clientId)
			.SendAsync(methodName, taskId, arg1, arg2, arg3, arg4, arg5, CancellationToken.None)!;
		return await WaitTaskComplete<T>(taskId);
	}

	public async Task<T?> InvokeAsync<T>(string clientId, string methodName, object? arg1, object? arg2, object? arg3,
		object? arg4,
		object? arg5, object? arg6)
	{
		
		var taskId = await RegisterTask<T>();
		await Clients?.Client(clientId)
			.SendAsync(methodName, taskId, arg1, arg2, arg3, arg4, arg5, arg6, CancellationToken.None)!;
		return await WaitTaskComplete<T>(taskId);
	}

	public async Task<T?> InvokeAsync<T>(string clientId, string methodName, object? arg1, object? arg2, object? arg3,
		object? arg4,
		object? arg5, object? arg6, object? arg7)
	{
		
		var taskId = await RegisterTask<T>();
		await Clients?.Client(clientId)
			.SendAsync(methodName, taskId, arg1, arg2, arg3, arg4, arg5, arg6, arg7, CancellationToken.None)!;
		return await WaitTaskComplete<T>(taskId);
	}

	public async Task<T?> InvokeAsync<T>(string clientId, string methodName, object? arg1, object? arg2, object? arg3,
		object? arg4,
		object? arg5, object? arg6, object? arg7, object? arg8)
	{

		var taskId = await RegisterTask<T>();
		await Clients?.Client(clientId)
			.SendAsync(methodName, taskId, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, CancellationToken.None)!;
		return await WaitTaskComplete<T>(taskId);
	}

	public async Task<T?> InvokeAsync<T>(string clientId, string methodName, object? arg1, object? arg2, object? arg3,
		object? arg4,
		object? arg5, object? arg6, object? arg7, object? arg8, object? arg9)
	{
		var taskId = await RegisterTask<T>();
		await Clients?.Client(clientId)
			.SendAsync(methodName, taskId, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9,
				CancellationToken.None)!;
		return await WaitTaskComplete<T>(taskId);
	}

	private async Task<string> RegisterTask<T>()
	{
		var taskId = Guid.NewGuid().ToString("N");
		var data = new SignalRData();
		data.DataType = typeof(T);
		while (!_taskHash.TryAdd(taskId, data))
		{
			await Task.Delay(10);
		}

		return taskId;
	}

	private async Task<T?> WaitTaskComplete<T>(string taskId)
	{
		if (_taskHash.TryGetValue(taskId, out var item))
		{
			while (!item.IsComplete) await Task.Delay(10);

			_taskHash.TryRemove(taskId,out item);

			if (item!.Data is T result) return result;

			return default;
		}

		return default;
	}
}