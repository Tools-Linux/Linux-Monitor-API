using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Linux_Monitor_API.Services.Network;

public class NetworkServices
{
    private readonly Dictionary<string, (ulong Rx, ulong Tx)> _previous = new();

    public List<object> GetSnapshot()
    {
        var interfaces = new List<object>();

        foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
        {
            try
            {
                var name = nic.Name;

                var rxPath = $"/sys/class/net/{name}/statistics/rx_bytes";
                var txPath = $"/sys/class/net/{name}/statistics/tx_bytes";

                if (!File.Exists(rxPath) || !File.Exists(txPath))
                    continue;

                ulong rxBytes = ulong.Parse(File.ReadAllText(rxPath));
                ulong txBytes = ulong.Parse(File.ReadAllText(txPath));

                double rxMbps = 0;
                double txMbps = 0;

                if (_previous.TryGetValue(name, out var old))
                {
                    rxMbps = ((rxBytes - old.Rx) * 8d) / 1_000_000d;
                    txMbps = ((txBytes - old.Tx) * 8d) / 1_000_000d;
                }

                _previous[name] = (rxBytes, txBytes);

                var ip = nic.GetIPProperties()
                    .UnicastAddresses
                    .FirstOrDefault(x => x.Address.AddressFamily == AddressFamily.InterNetwork)
                    ?.Address
                    .ToString() ?? "";

                var mac = string.Join(":",
                    nic.GetPhysicalAddress()
                        .GetAddressBytes()
                        .Select(x => x.ToString("X2")));

                interfaces.Add(new
                {
                    name,
                    ip,
                    mac,
                    status = nic.OperationalStatus == OperationalStatus.Up ? "up" : "down",
                    speedMbps = nic.Speed > 0 ? nic.Speed / 1_000_000 : 0,

                    rxMbps = Math.Round(rxMbps, 2),
                    txMbps = Math.Round(txMbps, 2),

                    rxBytes,
                    txBytes
                });
            }
            catch
            {
            }
        }

        return interfaces;
    }

    public Task<List<object>> GetNetworkAsync()
    {
        return Task.FromResult(GetSnapshot());
    }
}