using System.Globalization;
using System.Runtime.InteropServices;
using Linux_Monitor_API.Models.CPU;
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
        
        double cpuTemp = int.Parse(System.IO.File.ReadAllText("/sys/class/thermal/thermal_zone0/temp")) / 1000.0;
        
        var firstStats = ReadCpuStats();

        await Task.Delay(100);

        var secondStats = ReadCpuStats();

        var result = new List<CpuCoreUsage>(firstStats.Count);

        foreach (var (core, firstss) in firstStats)
        {
            if (!secondStats.TryGetValue(core, out var seconds))
                continue;

            var totalDelta = seconds.Total - firstss.Total;

            result.Add(new CpuCoreUsage
            {
                Core = core,
                Usage = totalDelta == 0
                    ? 0
                    : Math.Round(
                        100.0 * (totalDelta - (seconds.Idle - firstss.Idle)) / totalDelta,
                        1)
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
            tempCpu = cpuTemp,
            charge = result
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
    
    private static Dictionary<string, (ulong Idle, ulong Total)> ReadCpuStats()
    {
        var stats = new Dictionary<string, (ulong, ulong)>();

        foreach (var line in System.IO.File.ReadLines("/proc/stat"))
        {
            if (!line.StartsWith("cpu") || line.StartsWith("cpu "))
                continue;

            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            ulong user = ulong.Parse(parts[1], CultureInfo.InvariantCulture);
            ulong nice = ulong.Parse(parts[2], CultureInfo.InvariantCulture);
            ulong system = ulong.Parse(parts[3], CultureInfo.InvariantCulture);
            ulong idle = ulong.Parse(parts[4], CultureInfo.InvariantCulture);
            ulong iowait = ulong.Parse(parts[5], CultureInfo.InvariantCulture);
            ulong irq = ulong.Parse(parts[6], CultureInfo.InvariantCulture);
            ulong softirq = ulong.Parse(parts[7], CultureInfo.InvariantCulture);
            ulong steal = parts.Length > 8 ? ulong.Parse(parts[8], CultureInfo.InvariantCulture) : 0;

            ulong total = user + nice + system + idle + iowait + irq + softirq + steal;

            stats[parts[0]] = (idle + iowait, total);
        }

        return stats;
    }
}