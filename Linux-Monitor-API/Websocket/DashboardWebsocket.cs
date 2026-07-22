using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Linux_Monitor_API.Services.CPU;
using Linux_Monitor_API.Services.Logs;
using Linux_Monitor_API.Services.Memory;

namespace Linux_Monitor_API.Websocket;

public class DashboardWebSocket
{
    private readonly MemoryServices _memoryService;
    private readonly CpuServices _cpusServices;
    private readonly LogsServices _logsServices;

    private readonly ConcurrentBag<WebSocket> _clients = new();

    public DashboardWebSocket(MemoryServices memoryService, CpuServices cpusServices, LogsServices logsServices)
    {
        _memoryService = memoryService;
        _cpusServices = cpusServices;
        _logsServices = logsServices;
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
                var cpu = await _cpusServices.Get();


                await BroadcastAsync(new
                {
                    type = "dashboard",
                    memory,
                    cpu,
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