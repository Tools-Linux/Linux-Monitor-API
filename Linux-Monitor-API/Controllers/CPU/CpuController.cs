using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
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
        
        string cpuCore = System.IO.File.ReadLines("/proc/cpuinfo")
            .FirstOrDefault(l => l.StartsWith("cpu family"))
            ?.Split(':', 2)[1]
            .Trim() ?? "Unknown";

        string architecture = RuntimeInformation.ProcessArchitecture.ToString();

        int processcount = Directory.EnumerateDirectories("/proc")
            .Count(dir => int.TryParse(Path.GetFileName(dir), out _));
        
        
        int threadCount = 0;

        foreach (var procDir in Directory.EnumerateDirectories("/proc"))
        {
            if (!int.TryParse(Path.GetFileName(procDir), out _))
                continue;

            var taskDir = Path.Combine(procDir, "task");

            if (Directory.Exists(taskDir))
            {
                threadCount += Directory.EnumerateDirectories(taskDir).Count();
            }
        }
        
        return Ok(new
        {
            usage = Math.Round(usage, 2),
            name = cpuName,
            core = cpuCore,
            arch = architecture,
            processes = processcount,
            threads = threadCount,
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