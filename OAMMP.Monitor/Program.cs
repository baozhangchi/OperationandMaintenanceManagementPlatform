using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OAMMP.Common;
using OAMMP.Models;
using OAMMP.Monitor;
using OAMMP.Monitor.BackgroundServices;
using SqlSugar;

var host = new HostBuilder().ConfigureHostConfiguration(builder =>
    {
        builder
            .AddJsonFile("appsettings.json", false, false);

        if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ENVIRONMENT")))
        {
            builder
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ENVIRONMENT")}.json", true, false);
        }

        var homeConfigFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal,
            Environment.SpecialFolderOption.Create), ".monitor");
        if (Directory.Exists(homeConfigFolder))
        {
            var homeConfigFile = Path.Combine(homeConfigFolder, "appsettings.json");
            if (File.Exists(homeConfigFile))
            {
                builder.AddJsonFile(homeConfigFile, true, false);
            }
        }

        var configFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "monitor");
        if (Directory.Exists(configFolder))
        {
            var configFile = Path.Combine(configFolder, "appsettings.json");
            if (File.Exists(configFile))
            {
                builder.AddJsonFile(configFile, true, false);
            }
        }
    })
    .ConfigureLogging((hostContext, builder) =>
    {
        builder.AddConfiguration(hostContext.Configuration.GetSection("Logging"));
        builder.ClearProviders()
            .AddDebug()
            .AddConsole();
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.AddScoped<ISqlSugarClient>(s =>
        {
            var client = new SqlSugarClient(new ConnectionConfig
            {
                DbType = DbType.Sqlite,
                ConnectionString = $"DataSource={GlobalCache.DataSource}",
                IsAutoCloseConnection = true,
                ConfigureExternalServices = Repository.ExternalServices
            }, db =>
            {
                db.Aop.OnLogExecuting = (format, parameters) =>
                {
                    var sql = UtilMethods.GetSqlString(DbType.Sqlite, format, parameters);
                    s.GetRequiredService<ILogger<ISqlSugarClient>>().LogDebug(sql);
                };
                db.Aop.OnError = exception =>
                {
                    var sql = UtilMethods.GetSqlString(DbType.Sqlite, exception.Sql,
                        (SugarParameter[])exception.Parametres);
                    s.GetRequiredService<ILogger<ISqlSugarClient>>().LogError(sql);
                };
            });
            client.DbMaintenance.CreateDatabase();
            client.CodeFirst.InitTables(typeof(TableBase).Assembly.GetTypes()
                .Where(x => !x.IsAbstract && typeof(TableBase).IsAssignableFrom(x)).ToArray());
            return client;
        });
        services.AddScoped<Func<DateTime, ISqlSugarClient>>(s =>
        {
            return day =>
            {
                var regex = new Regex(@"(?<from>\d{8})-(?<to>\d{8}).db");
                var dbFiles = new DirectoryInfo(GlobalCache.DataFolder).GetFiles("*.db.bak")
                    .OrderByDescending(x => x.CreationTime).ToList();
                var dbFileMap = dbFiles
                    .Select(x =>
                    {
                        var match = regex.Match(x.Name);
                        var from = DateTime.ParseExact(match.Groups["from"].Value, @"yyyyMMdd",
                            CultureInfo.InvariantCulture);
                        var to = DateTime.ParseExact(match.Groups["to"].Value, @"yyyyMMdd",
                            CultureInfo.InvariantCulture);
                        return new { File = x, From = from, To = to };
                    }).ToList();
                var dbFile = dbFileMap.FirstOrDefault(x => x.From <= day.Date && x.To > day.Date)?.File ?? new FileInfo(
                    Path.Combine(GlobalCache.DataFolder,
                        $"{day:yyyyMMdd}-{day.AddDays(7):yyyyMMdd}.db.bak"));

                var client = new SqlSugarClient(
                    new ConnectionConfig
                    {
                        DbType = DbType.Sqlite,
                        ConnectionString = $"DataSource={dbFile.FullName}",
                        IsAutoCloseConnection = true,
                        ConfigureExternalServices = Repository.ExternalServices
                    }, db =>
                    {
                        db.Aop.OnLogExecuting = (format, parameters) =>
                        {
                            var sql = UtilMethods.GetSqlString(DbType.Sqlite, format, parameters);
                            s.GetRequiredService<ILogger<ISqlSugarClient>>().LogDebug(sql);
                        };
                        db.Aop.OnError = exception =>
                        {
                            var sql = UtilMethods.GetSqlString(DbType.Sqlite, exception.Sql,
                                (SugarParameter[])exception.Parametres);
                            s.GetRequiredService<ILogger<ISqlSugarClient>>().LogError(sql);
                        };
                    });
                client.DbMaintenance.CreateDatabase();
                client.CodeFirst.InitTables(typeof(LogTableBase).Assembly.GetTypes()
                    .Where(x => !x.IsAbstract && typeof(LogTableBase).IsAssignableFrom(x)).ToArray());
                return client;
            };
        });
        services.AddScoped(typeof(Repository<>));
        services.AddScoped(typeof(LogRepository<>));
        services.AddScoped(typeof(BackupRepository<>));
        services.AddScoped<IMonitorClientHandler, MonitorClientHandler>();
        services.AddScoped<SignalRClient<IMonitorClientHandler>, HostExtensions.SignalRClient>();
        services.AddHostedService<ServerResourceMonitoringService>();
        //services.AddHostedService<ApplicationResourceMonitoringService>();
    }).UseConsoleLifetime()
    .Build();
var configuration = host.Services.GetRequiredService<IConfiguration>();
var dataFolder = configuration["DataFolder"]!;
if (string.IsNullOrWhiteSpace(dataFolder))
{
    dataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "monitor");
}

GlobalCache.DataFolder = dataFolder;
host.ConnectSignalRHub();
host.Run();