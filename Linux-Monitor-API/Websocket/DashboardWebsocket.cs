using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Linux_Monitor_API.Services.CPU;
using Linux_Monitor_API.Services.Disk;
using Linux_Monitor_API.Services.Information;
using Linux_Monitor_API.Services.Memory;
using Linux_Monitor_API.Services.Network;

namespace Linux_Monitor_API.Websocket;

public class DashboardWebSocket
{
    private readonly MemoryServices _memoryService;
    private readonly CpuServices _cpusServices;
    private readonly DiskServices _diskServices;
    private readonly InformationServices _informationServices;
    private readonly NetworkServices _networkServices;

    private readonly ConcurrentBag<WebSocket> _clients = new();

    public DashboardWebSocket(MemoryServices memoryService, CpuServices cpusServices, DiskServices diskServices, InformationServices informationServices, NetworkServices networkServices)
    {
        _memoryService = memoryService;
        _cpusServices = cpusServices;
        _diskServices = diskServices;
        _informationServices = informationServices;
        _networkServices = networkServices;
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
                var disk = await _diskServices.Get();
                var information = await _informationServices.Get();
                var network = await _networkServices.GetNetworkAsync(); 


                await BroadcastAsync(new
                {
                    type = "dashboard",
                    memory,
                    cpu,
                    disk,
                    information,
                    network
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