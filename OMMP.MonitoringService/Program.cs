using OMMP.MonitoringService;
using OMMP.Common;
using OMMP.Models;
using OMMP.MonitoringService.BackgroundServices;
using SqlSugar;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton(GlobalCache.Client);
builder.Services.AddScoped<ISqlSugarClient>(context => RepositoryBase.GetClient());
builder.Services.AddHostedService<ServerResourceMonitoringService>()
    .AddHostedService<ApplicationResourceMonitoringService>();
builder.Services.AddScoped(typeof(Repository<>));
builder.Services.AddScoped(typeof(LogRepository<>));

var dataFolder = builder.Configuration["DataFolder"];
if (string.IsNullOrWhiteSpace(dataFolder))
{
    dataFolder = AppDomain.CurrentDomain.BaseDirectory;
}

GlobalCache.DataFolder = dataFolder;

if (!Directory.Exists(dataFolder))
{
    Directory.CreateDirectory(dataFolder);
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using (var repository = app.Services.CreateScope().ServiceProvider.GetService<LogRepository<NetworkLog>>())
{
    var ipAddress = HardwareHelper.GetIpAddresses();
    await repository.InsertRangeAsync(ipAddress.Select(x => new NetworkLog()
    {
        NetworkCardName = x.Item1,
        IpAddress = x.Item2,
        Time = DateTime.Now
    }).ToList());
}

app.Run();