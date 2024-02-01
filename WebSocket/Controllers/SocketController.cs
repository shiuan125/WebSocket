using Microsoft.AspNetCore.Mvc;
using WebSocket.Service;

namespace WebSocket.Controllers
{
    public class SocketController : Controller
    {
        [HttpGet("/ws")]
        public async Task Get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                var wsHandler = HttpContext.RequestServices.GetRequiredService<WebSocketHandler>();
                await wsHandler.ProcessWebSocket(webSocket);
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }
    }
}

