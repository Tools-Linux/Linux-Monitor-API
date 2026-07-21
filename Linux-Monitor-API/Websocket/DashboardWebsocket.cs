using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Linux_Monitor_API.Services.Memory;

namespace Linux_Monitor_API.Websocket;

public class DashboardWebSocket
{
    private readonly MemoryServices _memoryService;

    public DashboardWebSocket(
        MemoryServices memoryService)
    {
        _memoryService = memoryService;
    }

    public async Task HandleAsync(WebSocket socket, CancellationToken cancellationToken)
    {
        var receiveBuffer = new byte[1024];

        while (socket.State == WebSocketState.Open)
        { 
            var memory = _memoryService.GetAsync();

            var payload = new
            {
                memory
            };

            var json = JsonSerializer.Serialize(payload);

            await socket.SendAsync(
                Encoding.UTF8.GetBytes(json),
                WebSocketMessageType.Text,
                true,
                cancellationToken);

            await Task.Delay(1000, cancellationToken);
        }
    }
}