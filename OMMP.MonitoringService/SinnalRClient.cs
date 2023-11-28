using Microsoft.AspNetCore.SignalR.Client;
using OMMP.Common;
using OMMP.Models;
using SqlSugar;

namespace OMMP.MonitoringService;

public class SinnalRClient : IMonitoringClientHub
{
    private bool _connected;
    private HubConnection _connection;
    private Uri _uri;

    private SinnalRClient()
    {
    }

    public async void Connect(Uri uri)
    {
        _uri = uri;
        await ConnectAsync();
    }

    private async Task ConnectAsync()
    {
        while (!_connected)
        {
            try
            {
                _connection = new HubConnectionBuilder()
                    .WithUrl(_uri)
                    .Build();
                await _connection.StartAsync();
                Console.WriteLine("连接成功");
                _connected = true;
                _connection.On(nameof(IMonitoringClientHub.GetApplications), GetApplications);
                _connection.On<ApplicationInfo, bool>(nameof(IMonitoringClientHub.SaveApplication), SaveApplication);
                _connection.On<QueryLogArgs, List<CpuLog>>(nameof(IMonitoringClientHub.GetCpuLogs), GetCpuLogs);
                _connection.On<QueryLogArgs, List<MemoryLog>>(nameof(IMonitoringClientHub.GetMemoryLogs),
                    GetMemoryLogs);
                _connection.On<QueryLogArgs, Dictionary<string, List<NetworkRateLog>>>(
                    nameof(IMonitoringClientHub.GetNetworkRateLogs), GetNetworkRateLogs);
                _connection.On<QueryLogArgs, List<ServerResourceLog>>(
                    nameof(IMonitoringClientHub.GetServerResourceLogs), GetServerResourceLogs);
                _connection.On<string, DriveLog>(
                    nameof(IMonitoringClientHub.GetPartitionLog), GetPartitionLog);
                _connection.On(nameof(IMonitoringClientHub.GetPartitions), GetPartitions);
                _connection.On<long, QueryLogArgs, List<ApplicationLog>>(
                    nameof(IMonitoringClientHub.GetApplicationLogs), GetApplicationLogs);
                _connection.Closed += ReConnect;
                await _connection.SendAsync("RegisterClient", "MonitorClient",CancellationToken.None);
            }
            catch (HttpRequestException httpRequestException)
            {
                _connected = false;
            }
            catch (System.Net.WebSockets.WebSocketException webSocketException)
            {
                _connected = false;
                await ConnectAsync();
            }
        }
    }

    private async Task ReConnect(Exception arg)
    {
        Console.WriteLine("断开连接");
        _connection.Closed -= ReConnect;
        _connected = false;

        await ConnectAsync();
    }

    public static SinnalRClient Instance { get; } = new();

    public async Task<List<ApplicationInfo>> GetApplications()
    {
        var client = RepositoryBase.GetClient();
        var repository = new Repository<ApplicationInfo>(client);
        var items = await repository.GetListAsync();
        return items;
    }

    public async Task<List<ApplicationLog>> GetApplicationLogs(long applicationId, QueryLogArgs queryLogArgs)
    {
        var client = RepositoryBase.GetClient();
        var repository = LogRepository<ApplicationLog>.CreateInstance(client);
        var expression = new Expressionable<ApplicationLog>();
        expression.And(x => x.ApplicationId == applicationId);
        expression.AndIF(queryLogArgs.StartTime.HasValue, x => x.Time > queryLogArgs.StartTime.Value);
        expression.AndIF(queryLogArgs.EndTime.HasValue, x => x.Time <= queryLogArgs.EndTime.Value);
        if (queryLogArgs.Count.HasValue)
        {
            var items = await repository.GetLatestListAsync(expression.ToExpression(), queryLogArgs.Count.Value);
            return items;
        }
        else
        {
            var items = await repository.GetLatestListAsync(expression.ToExpression());
            return items;
        }
    }

    public async Task<bool> SaveApplication(ApplicationInfo application)
    {
        var client = RepositoryBase.GetClient();
        var repository = new Repository<ApplicationInfo>(client);
        return await repository.InsertOrUpdateAsync(application);
    }

    public async Task<List<CpuLog>> GetCpuLogs(QueryLogArgs args)
    {
        return await GetLogData<CpuLog>(args);
    }

    public async Task<List<MemoryLog>> GetMemoryLogs(QueryLogArgs args)
    {
        return await GetLogData<MemoryLog>(args);
    }

    public async Task<Dictionary<string, List<NetworkRateLog>>> GetNetworkRateLogs(QueryLogArgs args)
    {
        var client = RepositoryBase.GetClient();
        var repository = LogRepository<NetworkRateLog>.CreateInstance(client);

        var items = new Dictionary<string, List<NetworkRateLog>>();
        foreach (var networkCardName in HardwareHelper.NetworkCardNames)
        {
            var expression = new Expressionable<NetworkRateLog>();
            expression.And(x => x.NetworkCard == networkCardName);
            expression.AndIF(args.StartTime.HasValue, x => x.Time > args.StartTime.Value);
            expression.AndIF(args.EndTime.HasValue, x => x.Time <= args.EndTime.Value);
            if (args.Count.HasValue)
            {
                items.Add(networkCardName,
                    await repository.GetLatestListAsync(expression.ToExpression(), args.Count.Value));
            }
            else
            {
                items.Add(networkCardName,
                    await repository.GetLatestListAsync(expression.ToExpression()));
            }
        }

        return items;
    }

    public async Task<List<ServerResourceLog>> GetServerResourceLogs(QueryLogArgs args)
    {
        return await GetLogData<ServerResourceLog>(args);
    }

    public async Task<DriveLog> GetPartitionLog(string drive)
    {
        var client = RepositoryBase.GetClient();
        var repository = LogRepository<DriveLog>.CreateInstance(client);
        return await repository.GetLatestAsync(x => x.Name == drive);
    }

    public async Task<List<string>> GetPartitions()
    {
        return await Task.FromResult(HardwareHelper.Disks);
    }

    private async Task<List<T>> GetLogData<T>(QueryLogArgs args)
        where T : LogTableBase, new()
    {
        var client = RepositoryBase.GetClient();
        var repository = LogRepository<T>.CreateInstance(client);
        var expression = new Expressionable<T>();

        expression.AndIF(args.StartTime.HasValue, x => x.Time > args.StartTime.Value);
        expression.AndIF(args.EndTime.HasValue, x => x.Time <= args.EndTime.Value);
        if (args.Count.HasValue)
        {
            var items = await repository.GetLatestListAsync(expression.ToExpression(), args.Count.Value);
            return items;
        }
        else
        {
            var items = await repository.GetLatestListAsync(expression.ToExpression());
            return items;
        }
    }
}

public static class WebSocketClientExtensions
{
    public static WebApplication UseWebSocketClient(this WebApplication app)
    {
        var config = app.Services.GetService<IConfiguration>();
        var serviceUri = config["ServiceUri"];
        SinnalRClient.Instance.Connect(new UriBuilder(serviceUri)
        {
            Query = $"url={app.Urls.First()}"
        }.Uri);
        return app;
    }
}