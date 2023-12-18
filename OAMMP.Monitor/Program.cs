using System.Globalization;
using System.Text.RegularExpressions;
using OAMMP.Common;
using OAMMP.Models;
using OAMMP.Monitor;
using OAMMP.Monitor.BackgroundServices;
using SqlSugar;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddScoped<ISqlSugarClient>(s =>
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
builder.Services.AddScoped<Func<DateTime, ISqlSugarClient>>(s =>
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
builder.Services.AddScoped(typeof(Repository<>));
builder.Services.AddScoped(typeof(LogRepository<>));
builder.Services.AddScoped(typeof(BackupRepository<>));
builder.Services.AddScoped<IMonitorClientHandler, MonitorClientHandler>();
builder.Services.AddScoped(typeof(SignalRClient<>));
builder.Services.AddHostedService<ServerResourceMonitoringService>();
//builder.Services.AddHostedService<ApplicationResourceMonitoringService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();
var configuration = app.Services.GetRequiredService<IConfiguration>();
var dataFolder = configuration["DataFolder"]!;
if (string.IsNullOrWhiteSpace(dataFolder))
	dataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
		"monitor");

GlobalCache.DataFolder = dataFolder;
await app.StartAsync();
app.ConnectSignalRHub();
await app.WaitForShutdownAsync();