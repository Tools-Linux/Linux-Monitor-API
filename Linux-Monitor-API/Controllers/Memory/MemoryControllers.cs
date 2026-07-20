using Microsoft.AspNetCore.Mvc;

namespace Linux_Monitor_API.Controllers.Memory;

[ApiController]
[Route("api/memory")]
public class MemoryControllers : ControllerBase
{
    [HttpGet]
    public  IActionResult GetMemory()
    {
        var lines = System.IO.File.ReadAllLines("/proc/meminfo");

        long total = 0;
        long available = 0;

        foreach (var line in lines)
        {
            if (line.StartsWith("MemTotal:"))
            {
                total = ParseKb(line);
            }
            else if (line.StartsWith("MemAvailable:"))
            {
                available = ParseKb(line);
            }
        }
        
        var used = total - available;

        return Ok(new{
            totalMb = total / 1024,
            usedMb = used / 1024,
            availableMb = available / 1024,
            usagePercent = Math.Round((double)used / total * 100, 2)
        });
    }

    private static long ParseKb(string line)
    {
        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 2)
            throw new FormatException($"Invalid meminfo line: {line}");

        return long.Parse(parts[1]);
    }
}