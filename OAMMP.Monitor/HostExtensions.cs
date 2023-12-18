using OAMMP.Common;

namespace OAMMP.Monitor;

internal static class HostExtensions
{
	public static WebApplication ConnectSignalRHub(this WebApplication app)
	{
		var client = app.Services.CreateAsyncScope().ServiceProvider.GetRequiredService<SignalRClient<IMonitorClientHandler>>();
		var configuration = app.Services.GetRequiredService<IConfiguration>();
		client.Connect(new UriBuilder(configuration["ServiceUri"]!)
		{
			Query = $"url={app.Urls.First()}"
		}.Uri);
		return app;
	}
}