using System.Net.NetworkInformation;
using System.Net.Sockets;
using Microsoft.AspNetCore.Mvc;

namespace Linux_Monitor_API.Controllers.Network;

[ApiController]
[Route("api/network")]
public class NetworkController : ControllerBase
{
    private static readonly Dictionary<string, (ulong Rx, ulong Tx)> Previous = new();
    
    public static List<object> GetSnapshot()
    {
        var interfaces = new List<object>();

        foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
        {
            try
            {
                var name = nic.Name;

                var rxPath = $"/sys/class/net/{name}/statistics/rx_bytes";
                var txPath = $"/sys/class/net/{name}/statistics/tx_bytes";

                if (!System.IO.File.Exists(rxPath) ||
                    !System.IO.File.Exists(txPath)) 
                    continue;


                ulong rxBytes = ulong.Parse(System.IO.File.ReadAllText(rxPath));

                ulong txBytes = ulong.Parse(System.IO.File.ReadAllText(txPath));


                double rxMbps = 0;
                double txMbps = 0;


                if (Previous.TryGetValue(name, out var old))
                {
                    rxMbps = ((rxBytes - old.Rx) * 8d) / 1_000_000d;
                    txMbps = ((txBytes - old.Tx) * 8d) / 1_000_000d;
                }
                
                Previous[name] = (
                    rxBytes,
                    txBytes
                );

                var ip = nic.GetIPProperties()
                    .UnicastAddresses
                    .FirstOrDefault(x =>
                        x.Address.AddressFamily == AddressFamily.InterNetwork)
                    ?.Address
                    .ToString() ?? "";


                var mac = string.Join(":",
                    nic.GetPhysicalAddress()
                        .GetAddressBytes()
                        .Select(x => x.ToString("X2"))
                );
                
                interfaces.Add(new
                {
                    name,
                    ip,
                    mac,
                    status = nic.OperationalStatus == OperationalStatus.Up? "up" : "down",
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
    
    [HttpGet]
    public IActionResult GetNetwork()
    {
        
        return Ok(GetSnapshot());
    }
}