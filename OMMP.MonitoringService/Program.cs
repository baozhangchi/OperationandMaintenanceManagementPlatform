using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using OMMP.MonitoringService;
using OMMP.MonitoringService.BackgroundServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// builder.Services.AddSingleton(GlobalCache.Client);
builder.Services.AddScoped(context => RepositoryBase.GetClient());
builder.Services.AddHostedService<ServerResourceMonitoringService>()
    .AddHostedService<ApplicationResourceMonitoringService>();
builder.Services.AddScoped(typeof(Repository<>));
builder.Services.AddScoped(typeof(LogRepository<>));
builder.Services.AddSingleton(serviceProvider =>
{
    var server = serviceProvider.GetRequiredService<IServer>();
    return server.Features.Get<IServerAddressesFeature>();
});

var dataFolder = builder.Configuration["DataFolder"];
if (string.IsNullOrWhiteSpace(dataFolder)) dataFolder = AppDomain.CurrentDomain.BaseDirectory;

GlobalCache.DataFolder = dataFolder;

if (!Directory.Exists(dataFolder)) Directory.CreateDirectory(dataFolder);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.StartAsync();

app.UseWebSocketClient();

await app.WaitForShutdownAsync();

// app.Run();