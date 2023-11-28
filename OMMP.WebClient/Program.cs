using System.Reflection;
using OMMP.Models;
using OMMP.WebClient;
using OMMP.WebClient.Hubs;
using OMMP.WebClient.States;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddBootstrapBlazor();
builder.Services.AddSingleton(new GlobalCache());
builder.Services.AddSingleton<IClientState, ClientState>();
builder.Services.AddSingleton<IMonitorState, MonitorState>();
builder.Services.AddSignalR(o => { o.MaximumReceiveMessageSize = 1024 * 1024; });
var dataFolder = builder.Configuration["DataFolder"];
if (string.IsNullOrWhiteSpace(dataFolder)) dataFolder = AppDomain.CurrentDomain.BaseDirectory;

builder.Services.AddScoped(_ => RepositoryBase.GetClient(Path.Combine(dataFolder, $"system.db"),
    Assembly.GetExecutingAssembly().GetTypes().Where(x => typeof(TableBase).IsAssignableFrom(x)).ToArray()));

builder.Services.AddScoped(typeof(Repository<>));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.Services.GetRequiredService<GlobalCache>().DataFolder = dataFolder;
app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapHub<MonitoringHub>("/Monitoring");
app.MapHub<ClientHub>("/Client");
app.MapFallbackToPage("/_Host");

app.Run();