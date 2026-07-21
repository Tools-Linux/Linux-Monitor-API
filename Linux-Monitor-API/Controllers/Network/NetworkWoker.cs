using Microsoft.AspNetCore.SignalR;

namespace Linux_Monitor_API.Controllers.Network;

public class NetworkWorker : BackgroundService
{
    private readonly IHubContext<NetworkHub> _hub;

    public NetworkWorker(IHubContext<NetworkHub> hub)
    {
        _hub = hub;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var data = NetworkController.GetSnapshot();

            await _hub.Clients.All.SendAsync(
                "network",
                data,
                stoppingToken);

            await Task.Delay(1000, stoppingToken);
        }
    }
}