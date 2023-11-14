using System.Text;
using Microsoft.AspNetCore.WebSockets;
using OMMP.WebService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddWebSockets(options => { });
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

app.Use(async (context, func) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        GlobalCache.MonitoringClients.Add(webSocket);
        var bytes = new List<byte>();
        var buffer = new byte[1024 * 4];
        var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        while (!result.CloseStatus.HasValue)
        {
            while (!result.EndOfMessage)
            {
                bytes.AddRange(buffer);
                buffer = new byte[1024 * 4];
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            bytes.AddRange(buffer.Take(result.Count));

            var message = Encoding.UTF8.GetString(bytes.ToArray());
            
            var serverMsg = Encoding.UTF8.GetBytes($"Server: Hello. You said: {Encoding.UTF8.GetString(buffer)}");
            //向客户端发送消息
            await webSocket.SendAsync(new ArraySegment<byte>(serverMsg, 0, serverMsg.Length), result.MessageType,
                result.EndOfMessage, CancellationToken.None);
            //继续接受客户端消息
            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        //关闭释放与客户端连接
        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
    }
    else
    {
        await func.Invoke();
    }
});
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();