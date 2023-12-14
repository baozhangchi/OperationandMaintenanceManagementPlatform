using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OAMMP.Common;
using OAMMP.Models;

namespace OAMMP.Monitor;

internal static class HostExtensions
{
    public static IHost ConnectSignalRHub(this IHost host)
    {
        var client = host.Services.GetRequiredService<SignalRClient<IMonitorClientHandler>>();
        var configuration = host.Services.GetRequiredService<IConfiguration>();
        client.Connect(new Uri(configuration["ServiceUri"]!));
        return host;
    }

    public class SignalRClient : SignalRClient<IMonitorClientHandler>
    {
        public SignalRClient(IMonitorClientHandler clientHandler) : base(clientHandler)
        {
        }

        protected override void RegisterListenMethods(HubConnection hubConnection)
        {
            base.RegisterListenMethods(hubConnection);
            //hubConnection.On(nameof(IMonitorClientHandler.GetApplications), ClientHandler.GetApplications);
            //hubConnection.On<ApplicationItem, bool>(nameof(IMonitorClientHandler.SaveApplication), async o =>
            //{
            //    var result = await ClientHandler.SaveApplication(o);
            //    if (result)
            //    {
            //        await hubConnection.SendAsync("ApplicationsUpdated", await ClientHandler.GetApplications(),
            //            CancellationToken.None);
            //    }

            //    return result;
            //});
            //hubConnection.On<QueryLogArgs, List<CpuLog>>(nameof(IMonitorClientHandler.GetCpuLogs),
            //    ClientHandler.GetCpuLogs);
            //hubConnection.On<QueryLogArgs, List<MemoryLog>>(nameof(IMonitorClientHandler.GetMemoryLogs),
            //    ClientHandler.GetMemoryLogs);
            //hubConnection.On<QueryLogArgs, List<ServerResourceLog>>(
            //    nameof(IMonitorClientHandler.GetServerResourceLogs), ClientHandler.GetServerResourceLogs);
            //hubConnection.On<string, PartitionLog>(
            //    nameof(IMonitorClientHandler.GetPartitionLog), ClientHandler.GetPartitionLog);
            //hubConnection.On(nameof(IMonitorClientHandler.GetPartitions), ClientHandler.GetPartitions);
            //hubConnection.On<QueryLogArgs, List<NetworkLog>>(nameof(IMonitorClientHandler.GetNetworkLogs),
            //    ClientHandler.GetNetworkLogs);
            //hubConnection.On<long, bool>(nameof(IMonitorClientHandler.GetApplicationAlive),
            //    ClientHandler.GetApplicationAlive);
            //hubConnection.On<long, ApplicationItem>(nameof(IMonitorClientHandler.GetApplication),
            //    ClientHandler.GetApplication);
            //hubConnection.On<long, QueryLogArgs, List<ApplicationLog>>(
            //    nameof(IMonitorClientHandler.GetApplicationLogs), ClientHandler.GetApplicationLogs);
        }
    }
}