using System.Globalization;
using System.Runtime.InteropServices;
using Linux_Monitor_API.Models.CPU;

namespace Linux_Monitor_API.Services.CPU;

public class CpuServices
{
    public async Task<object> Get()
    {
        var first = ReadCpu();
        await Task.Delay(500);
        var second = ReadCpu();

        var idle = second.Idle - first.Idle;
        var total = second.Total - first.Total;
        var usage = total == 0 ? 0 : (1.0 - (double)idle / total) * 100;

        string cpuName = File.ReadLines("/proc/cpuinfo")
            .FirstOrDefault(l => l.StartsWith("model name"))
            ?.Split(':',2)[1].Trim() ?? "Unknown";

        string cpuCore = File.ReadLines("/proc/cpuinfo")
            .FirstOrDefault(l => l.StartsWith("cpu family"))
            ?.Split(':',2)[1].Trim() ?? "Unknown";

        string architecture = RuntimeInformation.ProcessArchitecture.ToString();

        int processCount = Directory.EnumerateDirectories("/proc")
            .Count(x => int.TryParse(Path.GetFileName(x),out _));

        int threadCount = 0;

        foreach(var proc in Directory.EnumerateDirectories("/proc"))
        {
            if(!int.TryParse(Path.GetFileName(proc),out _))
                continue;

            var task = Path.Combine(proc,"task");

            if(Directory.Exists(task))
                threadCount += Directory.EnumerateDirectories(task).Count();
        }

        string hostname = Environment.MachineName;

        string osName = File.ReadLines("/etc/os-release")
            .FirstOrDefault(x => x.StartsWith("PRETTY_NAME="))
            ?.Split('=',2)[1].Trim('"') ?? "Unknown";

        string kernel = File.ReadAllText("/proc/sys/kernel/osrelease").Trim();

        double? tempCpu = GetCpuTemperature();

        var firstStats = ReadCpuStats();

        await Task.Delay(100);

        var secondStats = ReadCpuStats();

        var cores = new List<CpuCoreUsage>();

        foreach(var cpu in firstStats)
        {
            if(!secondStats.TryGetValue(cpu.Key,out var secondCpu))
                continue;

            var totalDelta = secondCpu.Total - cpu.Value.Total;
            var idleDelta = secondCpu.Idle - cpu.Value.Idle;

            cores.Add(new CpuCoreUsage
            {
                Core = cpu.Key,
                Usage = totalDelta == 0 ? 0 :
                    Math.Round(100.0 * (totalDelta - idleDelta) / totalDelta,1)
            });
        }

        return new
        {
            usage = Math.Round(usage,2),
            name = cpuName,
            core = cpuCore,
            arch = architecture,
            processes = processCount,
            threads = threadCount,
            host = hostname,
            os = osName,
            kernel = kernel,
            tempCpu = tempCpu,
            charge = cores
        };
    }

    private static double? GetCpuTemperature()
    {
        var paths = new[]
        {
            "/sys/class/thermal/thermal_zone0/temp",
            "/sys/class/hwmon/hwmon0/temp1_input"
        };

        foreach(var path in paths)
        {
            if(!File.Exists(path))
                continue;

            try
            {
                var value = File.ReadAllText(path).Trim();

                if(double.TryParse(value,out var temp))
                    return Math.Round(temp > 1000 ? temp / 1000 : temp,1);
            }
            catch
            {
            }
        }

        return null;
    }

    private static (long Idle,long Total) ReadCpu()
    {
        var values = File.ReadLines("/proc/stat")
            .First()
            .Split(' ',StringSplitOptions.RemoveEmptyEntries)
            .Skip(1)
            .Select(long.Parse)
            .ToArray();

        return(values[3] + values[4],values.Sum());
    }

    private static Dictionary<string,(ulong Idle,ulong Total)> ReadCpuStats()
    {
        var stats = new Dictionary<string,(ulong,ulong)>();

        foreach(var line in File.ReadLines("/proc/stat"))
        {
            if(!line.StartsWith("cpu") || line.StartsWith("cpu "))
                continue;

            var p = line.Split(' ',StringSplitOptions.RemoveEmptyEntries);

            ulong user = ulong.Parse(p[1],CultureInfo.InvariantCulture);
            ulong nice = ulong.Parse(p[2],CultureInfo.InvariantCulture);
            ulong system = ulong.Parse(p[3],CultureInfo.InvariantCulture);
            ulong idle = ulong.Parse(p[4],CultureInfo.InvariantCulture);
            ulong io = ulong.Parse(p[5],CultureInfo.InvariantCulture);
            ulong irq = ulong.Parse(p[6],CultureInfo.InvariantCulture);
            ulong soft = ulong.Parse(p[7],CultureInfo.InvariantCulture);
            ulong steal = p.Length > 8 ? ulong.Parse(p[8],CultureInfo.InvariantCulture) : 0;

            stats[p[0]] = (
                idle + io,
                user + nice + system + idle + io + irq + soft + steal
            );
        }

        return stats;
    }
}