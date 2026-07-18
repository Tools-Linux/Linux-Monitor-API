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
        
        string hostname = Environment.MachineName;
        string osName = System.IO.File.ReadLines("/etc/os-release")
            .FirstOrDefault(l => l.StartsWith("PRETTY_NAME="))
            ?.Split('=', 2)[1]
            .Trim('"') ?? "Unknown";
        
        string kernelVersion = System.IO.File.ReadAllText("/proc/sys/kernel/osrelease").Trim();
        
        var thermalZones = Directory.GetDirectories("/sys/class/thermal");

        var temperatures = new List<object>();

        foreach (var zone in thermalZones)
        {
            var typeFile = Path.Combine(zone, "type");
            var tempFile = Path.Combine(zone, "temp");

            if (!System.IO.File.Exists(typeFile) || !System.IO.File.Exists(tempFile))
                continue;

            string type = System.IO.File.ReadAllText(typeFile).Trim();
            double temp = int.Parse(System.IO.File.ReadAllText(tempFile)) / 1000.0;

            temperatures.Add(new
            {
                zone = Path.GetFileName(zone),
                type,
                temperature = temp
            });
        }

        return Ok(new
        {
            usage = Math.Round(usage, 2),
            name = cpuName,
            core = cpuCore,
            arch = architecture,
            processes = processcount,
            threads = threadCount,
            host = hostname,
            os = osName,
            kernel = kernelVersion,
            temp = temperatures
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