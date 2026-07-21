using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Linux_Monitor_API.Services.Memory;

namespace Linux_Monitor_API.Websocket;

public class DashboardWebSocket
{
    private readonly MemoryServices _memoryService;

    private readonly ConcurrentDictionary<Guid, WebSocket> _clients = new();


    public DashboardWebSocket(
        MemoryServices memoryService)
    {
        _memoryService = memoryService;
    }


    public async Task HandleAsync(
        WebSocket socket,
        CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();

        _clients.TryAdd(id, socket);


        Console.WriteLine(
            $"Dashboard clients : {_clients.Count}"
        );


        try
        {
            while(socket.State == WebSocketState.Open)
            {
                await Task.Delay(
                    1000,
                    cancellationToken
                );


                var memory = await _memoryService.GetAsync();


                await BroadcastAsync(new
                {
                    type = "memory",
                    data = memory
                });
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            _clients.TryRemove(id, out _);


            if(socket.State == WebSocketState.Open)
            {
                await socket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Closed",
                    CancellationToken.None
                );
            }


            Console.WriteLine(
                $"Dashboard clients : {_clients.Count}"
            );
        }
    }


    private async Task BroadcastAsync(object data)
    {
        var json = JsonSerializer.Serialize(data);

        var buffer = Encoding.UTF8.GetBytes(json);


        foreach(var socket in _clients.Values)
        {
            if(socket.State != WebSocketState.Open)
                continue;


            await socket.SendAsync(
                buffer,
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
            );
        }
    }
}