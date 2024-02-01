using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace WebSocket.Service
{
    public class WebSocketHandler
    {
        //REF: https://radu-matei.com/blog/aspnet-core-websockets-middleware/
        ConcurrentDictionary<int, System.Net.WebSockets.WebSocket> WebSockets = new ConcurrentDictionary<int, System.Net.WebSockets.WebSocket>();

        public async Task ProcessWebSocket(System.Net.WebSockets.WebSocket webSocket)
        {
            WebSockets.TryAdd(webSocket.GetHashCode(), webSocket);
            var buffer = new byte[1024 * 4];
            var res = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            var userName = "anonymous";
            while (!res.CloseStatus.HasValue)
            {
                var cmd = Encoding.UTF8.GetString(buffer, 0, res.Count);
                if (!string.IsNullOrEmpty(cmd))
                {
                    if (cmd.StartsWith("/USER "))
                        userName = cmd.Substring(6);
                    else
                        Broadcast($"{userName}:\t{cmd}");
                }
                res = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            await webSocket.CloseAsync(res.CloseStatus.Value, res.CloseStatusDescription, CancellationToken.None);
            WebSockets.TryRemove(webSocket.GetHashCode(), out var removed);
            Broadcast($"{userName} left the room.");
        }

        public void Broadcast(string message)
        {
            var buff = Encoding.UTF8.GetBytes($"{DateTime.Now:MM-dd HH:mm:ss}\t{message}");
            var data = new ArraySegment<byte>(buff, 0, buff.Length);
            Parallel.ForEach(WebSockets.Values, async (webSocket) =>
            {
                if (webSocket.State == WebSocketState.Open)
                    await webSocket.SendAsync(data, WebSocketMessageType.Text, true, CancellationToken.None);
            });
        }
    }
}
