using System.Text;
using Microsoft.AspNetCore.WebSockets;
using OMMP.WebClient;
using OMMP.WebClient.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddBootstrapBlazor();
builder.Services.AddWebSockets(options => { options.KeepAliveInterval = TimeSpan.FromMinutes(5); });
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapHub<MonitoringHub>("/Monitoring");
app.MapFallbackToPage("/_Host");

app.Run();