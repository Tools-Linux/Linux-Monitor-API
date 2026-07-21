using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Linux_Monitor_API.Services.Memory;

namespace Linux_Monitor_API.Websocket;

public class DashboardWebSocket
{
    private readonly MemoryServices _memoryService;

    private readonly ConcurrentBag<WebSocket> _clients = new();

    public DashboardWebSocket(MemoryServices memoryService)
    {
        _memoryService = memoryService;
    }


    public async Task HandleAsync(
        WebSocket socket,
        CancellationToken cancellationToken)
    {
        _clients.Add(socket);

        Console.WriteLine($"Clients WS : {_clients.Count}");

        try
        {
            while (socket.State == WebSocketState.Open)
            {
                var memory = await _memoryService.GetAsync();


                await BroadcastAsync(new
                {
                    type = "memory",
                    data = memory
                });


                await Task.Delay(1000, cancellationToken);
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            _clients.TryTake(out _);

            Console.WriteLine($"Clients WS : {_clients.Count}");
        }
    }


    private async Task BroadcastAsync(object data)
    {
        var json = JsonSerializer.Serialize(data);

        var buffer = Encoding.UTF8.GetBytes(json);


        foreach(var client in _clients)
        {
            if(client.State != WebSocketState.Open)
                continue;


            await client.SendAsync(
                buffer,
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
            );
        }
    }
}