#region

using System.Collections;
using Microsoft.AspNetCore.SignalR;
using OAMMP.Client.Common;

#endregion

namespace OAMMP.Server.Hubs;

public interface IMonitorState : IEnumerable<MonitorServer>
{
	string this[string ip] { get; set; }

	IHubCallerClients? Clients { get; set; }

	void Remove(string ip);
}

public class MonitorState : IMonitorState
{
	private readonly List<MonitorServer> _data = new();
	private readonly ILogger<IMonitorState> _logger;

	public MonitorState(ILogger<IMonitorState> logger)
	{
		_logger = logger;
	}

	public IHubCallerClients? Clients { get; set; }

	public IEnumerator<MonitorServer> GetEnumerator()
	{
		return _data.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)_data).GetEnumerator();
	}

	public string this[string ip]
	{
		get => _data.SingleOrDefault(x => x.Ip == ip)?.Url ?? string.Empty;

		set
		{
			var item = _data.SingleOrDefault(x => x.Ip == ip);
			if (item != null)
				item.Url = value;
			else
				_data.Add(new MonitorServer
				{
					Ip = ip,
					Url = value
				});
		}
	}

	public void Remove(string ip)
	{
		_data.RemoveAll(x => x.Ip == ip);
	}
}