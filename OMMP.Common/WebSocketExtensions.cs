using System.Net.WebSockets;
using System.Text;

namespace OMMP.Common;

public static class WebSocketExtensions
{
    public static async void SendAsync(this WebSocket socket, string message, Encoding? encoding = null)
    {
        await socket.SendAsync((encoding ?? Encoding.UTF8).GetBytes(message), WebSocketMessageType.Text, true,
            CancellationToken.None);
    }
}