using NLog.Extensions.Logging;
using OAMMP.Client.Common;
using OAMMP.Common;
using OAMMP.Server.Hubs;
using IClientHandler = OAMMP.Client.Common.IClientHandler;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureLogging((context, loggingBuilder) =>
{
	loggingBuilder.ClearProviders();
	loggingBuilder.AddNLog();
});

// Add services to the container.
builder.Services.AddHttpClient();
builder.Services.AddRazorPages();
builder.Services.AddBootstrapBlazor(options => { });
builder.Services.AddServerSideBlazor(options =>
{
});
builder.Services.AddSignalR(options => { options.MaximumReceiveMessageSize = 1024 * 1024 * 1024; }).AddNewtonsoftJsonProtocol();
builder.Services.AddSingleton<IClientState, ClientState>();
builder.Services.AddSingleton<IMonitorState, MonitorState>();
builder.Services.AddSingleton<IClientHandler, ClientHandler>();
builder.Services.AddSingleton(typeof(SignalRClient<>));

// Add Telerik Blazor server side services

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

//app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();
app.MapControllers();
app.MapBlazorHub();
app.MapHub<ClientHub>("/Client");
app.MapHub<MonitorHub>("/Monitor");
app.MapFallbackToPage("/_Host");

app.Run();