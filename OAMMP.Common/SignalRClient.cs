using System.Net.WebSockets;
using Microsoft.AspNetCore.SignalR.Client;

namespace OAMMP.Common;

public class SignalRClient<T>
	where T : class
{
	public readonly T ClientHandler;
	public HubConnection? Connection { get; private set; }
	private Uri? _uri;

	public SignalRClient(T clientHandler)
	{
		ClientHandler = clientHandler;
	}

	public async void Connect(Uri? uri)
	{
		if (uri == null)
		{
			return;
		}

		_uri = uri;
		var builder = new HubConnectionBuilder();
		builder.WithUrl(_uri).WithAutomaticReconnect(new[]
			{ TimeSpan.Zero, TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10) });
		Connection = builder.Build();
		Connection.Closed += ReConnect;

		RegisterListenMethods(Connection);

		await ConnectHub();
	}

	protected virtual void RegisterListenMethods(HubConnection hubConnection)
	{
		var type = typeof(T);
		var methods = type.GetMethods();
		foreach (var method in methods)
		{
			var parameters = method.GetParameters();
			if (method.Name.StartsWith("set_") || method.Name.StartsWith("get_") || method.ReturnType != typeof(Task))
			{
				continue;
			}

			if (parameters.Length == 0)
			{
				hubConnection.On(method.Name, Type.EmptyTypes, _ => (Task)method.Invoke(ClientHandler, Array.Empty<object?>())!);
			}
			else
			{
				hubConnection.On(method.Name, parameters.Select(x => x.ParameterType).ToArray(), o => (Task)method.Invoke(ClientHandler, o)!);
			}
		}
	}

	private async Task ConnectHub()
	{
		if (Connection == null) return;
		try
		{
			await Connection.StartAsync();
			Console.WriteLine($"与服务器{_uri}连接成功");
		}
		catch (HttpRequestException httpRequestException)
		{
			Console.WriteLine(httpRequestException.GetBaseException().Message);
			await Task.Delay(TimeSpan.FromSeconds(10));
			await ConnectHub();
		}
		catch (WebSocketException webSocketException)
		{
			Console.WriteLine(webSocketException.GetBaseException().Message);
			await Task.Delay(TimeSpan.FromSeconds(10));
			await ConnectHub();
		}
		catch (Exception exception)
		{
			Console.WriteLine(exception);
			throw;
		}
	}

	private async Task ReConnect(Exception? arg)
	{
		Console.WriteLine($"与服务器{_uri}断开连接，开始重新连接");
		await ConnectHub();
	}
}