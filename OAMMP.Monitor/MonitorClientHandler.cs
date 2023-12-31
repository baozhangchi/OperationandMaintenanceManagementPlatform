﻿using System.Linq.Expressions;
using CZGL.SystemInfo;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using OAMMP.Common;
using OAMMP.Models;
using SqlSugar;

namespace OAMMP.Monitor;

internal class MonitorClientHandler : IMonitorClientHandler
{
    private readonly IServiceProvider _serviceProvider;

    public HubConnection? Connection { get; set; }

    public MonitorClientHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task GetApplications(string taskId)
    {
        using (var repository = _serviceProvider.CreateAsyncScope().ServiceProvider
                   .GetRequiredService<Repository<ApplicationItem>>())
        {
            await Callback(taskId, await repository.GetListAsync());
        }
    }

    private Task Callback<T>(string taskId, T? data)
    {
        return Connection!.SendAsync("Callback", taskId, data);
    }

    public async Task GetApplicationAlive(string taskId, long appId)
    {
        using (var repository =
               _serviceProvider.CreateAsyncScope().ServiceProvider.GetRequiredService<LogRepository<ApplicationLog>>())
        {
            var item = await repository.GetLatestAsync(x => x.ApplicationId == appId);
            await Callback(taskId, item.IsLive);
        }
    }

    public async Task GetApplication(string taskId, long appId)
    {
        using (var repository =
               _serviceProvider.CreateAsyncScope().ServiceProvider.GetRequiredService<Repository<ApplicationItem>>())
        {
            await Callback(taskId, await repository.GetSingleAsync(x => x.UUID == appId));
        }
    }

    public async Task GetApplicationLogs(string taskId, long applicationId, QueryLogArgs args)
    {
        await Callback(taskId, await GetLogData<ApplicationLog>(args, x => x.ApplicationId == applicationId));
    }

    public async Task SaveApplication(string taskId, ApplicationItem application)
    {
        if (!application.AutoRestart)
        {
            application.AutoRestartTimeValue = null;
        }

        using (var repository = _serviceProvider.CreateAsyncScope().ServiceProvider
                   .GetRequiredService<Repository<ApplicationItem>>())
        {
            await Callback(taskId, await repository.InsertOrUpdateAsync(application));
            await Connection!.SendAsync("ApplicationsUpdated", await repository.GetListAsync());
        }
    }

    public async Task GetCpuLogs(string taskId, QueryLogArgs args)
    {
        await Callback(taskId, await GetLogData<CpuLog>(args));
    }

    public async Task GetNetworkCards(string taskId)
    {
        await Callback(taskId, NetworkInfo.GetNetworkInfos().Where(x => x.IsSupportIpv4 && !string.IsNullOrWhiteSpace(x.Mac) && x.Status == System.Net.NetworkInformation.OperationalStatus.Up).ToDictionary(x => x.Mac, x => x.Name));
    }

    public async Task GetMemoryLogs(string taskId, QueryLogArgs args)
    {
        await Callback(taskId, await GetLogData<MemoryLog>(args));
    }

    public async Task GetServerResourceLogs(string taskId, QueryLogArgs args)
    {
        await Callback(taskId, await GetLogData<ServerResourceLog>(args));
    }

    public async Task GetPartitionLog(string taskId, string drive)
    {
        using (var repository =
               _serviceProvider.CreateAsyncScope().ServiceProvider.GetRequiredService<LogRepository<PartitionLog>>())
        {
            await Callback(taskId, await repository.GetLatestAsync(x => x.Name == drive.FromBase64()));
        }
    }

    public Task GetPartitions(string taskId)
    {
        return Callback(taskId, DriveInfo.GetDrives().Select(drive => drive.Name).ToList());
    }

    public Task UploadApplication(string taskId, ReadOnlyMemory<byte> buffer)
    {
        throw new NotImplementedException();
    }

    public Task GetApplicationBackup(string taskId)
    {
        throw new NotImplementedException();
    }

    public Task DownloadApplicationLogs(string taskId)
    {
        throw new NotImplementedException();
    }

    public async Task DeleteApplications(string taskId, List<long> applicationIds)
    {
        using (var repository =
               _serviceProvider.CreateAsyncScope().ServiceProvider.GetRequiredService<Repository<ApplicationItem>>())
        {
            var items = await repository.GetListAsync(x => applicationIds.Contains(x.UUID));
            await Callback(taskId, await repository.DeleteAsync(items));
            await Connection!.SendAsync("ApplicationsUpdated", await repository.GetListAsync());
        }
    }

    public async Task GetNetworkLogs(string taskId, QueryLogArgs args)
    {
        using (var repository =
               _serviceProvider.CreateAsyncScope().ServiceProvider.GetRequiredService<LogRepository<NetworkLog>>())
        {
            var data = new List<NetworkLog>();
            foreach (var networkInfo in NetworkInfo.GetNetworkInfos())
            {
                var mac = networkInfo.Mac;
                var expression = new Expressionable<NetworkLog>();
                expression.And(x => x.Mac == mac);
                expression.AndIF(args.StartTime.HasValue, x => x.Time > args.StartTime!.Value);
                expression.AndIF(args.EndTime.HasValue, x => x.Time <= args.EndTime!.Value);
                if (args.Count.HasValue)
                {
                    var items = await repository.GetLatestListAsync(expression.ToExpression(), args.Count.Value);
                    data.AddRange(items);
                }
                else
                {
                    var items = await repository.GetLatestListAsync(expression.ToExpression());
                    data.AddRange(items);
                }
            }

            await Callback(taskId, data);
        }
    }

    //public async Task GetNetworkLogs(string taskId, string mac, QueryLogArgs args)
    //{
    //    using (var repository =
    //           _serviceProvider.CreateAsyncScope().ServiceProvider.GetRequiredService<LogRepository<NetworkLog>>())
    //    {
    //        var expression = new Expressionable<NetworkLog>();
    //        expression.And(x => x.Mac == mac);
    //        expression.AndIF(args.StartTime.HasValue, x => x.Time > args.StartTime!.Value);
    //        expression.AndIF(args.EndTime.HasValue, x => x.Time <= args.EndTime!.Value);
    //        if (args.Count.HasValue)
    //        {
    //            var items = await repository.GetLatestListAsync(expression.ToExpression(), args.Count.Value);
    //            await Callback(taskId, items);
    //        }
    //        else
    //        {
    //            var items = await repository.GetLatestListAsync(expression.ToExpression());
    //            await Callback(taskId, items);
    //        }
    //    }
    //}


    private async Task<List<T>> GetLogData<T>(QueryLogArgs args, Expression<Func<T, bool>>? exp = null)
        where T : LogTableBase, new()
    {
        using (var repository =
               _serviceProvider.CreateAsyncScope().ServiceProvider.GetRequiredService<LogRepository<T>>())
        {
            var expression = new Expressionable<T>();
            if (exp != null)
            {
                expression.And(exp);
            }

            expression.AndIF(args.StartTime.HasValue, x => x.Time > args.StartTime!.Value);
            expression.AndIF(args.EndTime.HasValue, x => x.Time <= args.EndTime!.Value);
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
}