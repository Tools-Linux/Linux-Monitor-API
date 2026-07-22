using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Linux_Monitor_API.Services.Logs;
using Linux_Monitor_API.Services.Services;

namespace Linux_Monitor_API.Websocket;

public class ServiceWebsocket
{
    private readonly ProcessManager _serviceManager;
    private readonly ConcurrentBag<WebSocket> _clients = new();

    public ServiceWebsocket(ProcessManager serviceManager)
    {
        _serviceManager = serviceManager;
    }


    public async Task HandleAsync(
        WebSocket socket,
        CancellationToken cancellationToken)
    {
        _clients.Add(socket);

        Console.WriteLine($"Logs WS clients : {_clients.Count}");


        try
        {
            while(socket.State == WebSocketState.Open)
            {
                var services = await _serviceManager.GetProcesses();

                await SendAsync(socket,new
                {
                    type="logs",
                    services
                });


                await Task.Delay(
                    1000,
                    cancellationToken
                );
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            _clients.TryTake(out _);

            Console.WriteLine($"Logs WS clients : {_clients.Count}");
        }
    }


    private async Task SendAsync(
        WebSocket socket,
        object data)
    {
        var json = JsonSerializer.Serialize(data);

        await socket.SendAsync(
            Encoding.UTF8.GetBytes(json),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None
        );
    }
}