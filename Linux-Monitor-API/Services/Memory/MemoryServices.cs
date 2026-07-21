using Linux_Monitor_API.Models.Memory;

namespace Linux_Monitor_API.Services.Memory;

public class MemoryServices
{
    public Task<MemoryInfo> GetAsync()
    {
        var lines = File.ReadAllLines("/proc/meminfo");

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

        return Task.FromResult(new MemoryInfo
        {
            Total = total,
            Used = used,
            Available = available,
            Usage = total == 0
                ? 0
                : Math.Round((double)used / total * 100, 2)
        });
    }


    private static long ParseKb(string line)
    {
        var parts = line.Split(
            ' ',
            StringSplitOptions.RemoveEmptyEntries
        );

        if (parts.Length < 2)
            throw new FormatException(
                $"Invalid meminfo line: {line}"
            );

        return long.Parse(parts[1]);
    }
}