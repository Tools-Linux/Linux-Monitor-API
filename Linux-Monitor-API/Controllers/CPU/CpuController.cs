using Microsoft.AspNetCore.Mvc;

namespace Linux_Monitor_API.Controllers.CPU;

[ApiController]
[Route("api/cpu")]
public class CpuController  : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var first = ReadCpu();
        await Task.Delay(500);
        var second = ReadCpu();

        var idle = second.Idle - first.Idle;
        var total = second.Total - first.Total;

        var usage = (1.0 - (double)idle / total) * 100;
        
        
        string cpuName = System.IO.File.ReadLines("/proc/cpuinfo")
            .FirstOrDefault(l => l.StartsWith("model name"))
            ?.Split(':', 2)[1]
            .Trim() ?? "Unknown";
        
        return Ok(new
        {
            usage = Math.Round(usage, 2),
            name = cpuName
        });
    }
    
    private static (long Idle, long Total) ReadCpu()
    {
        var line = System.IO.File.ReadLines("/proc/stat").First();
        var values = line.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Skip(1)
            .Select(long.Parse)
            .ToArray();

        long idle = values[3] + values[4];
        long total = values.Sum();

        return (idle, total);
    }
}