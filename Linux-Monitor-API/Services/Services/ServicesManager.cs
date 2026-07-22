using System.Diagnostics;

namespace Linux_Monitor_API.Services.Services;

public class ProcessManager
{
    public async Task<List<object>> GetProcesses()
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "ps",
                Arguments = "-eo pid,user,state,comm,%cpu,rss,lstart --no-headers",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();

        string output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();

        var processes = output
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line =>
            {
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length < 7)
                    return null;

                int.TryParse(parts[0], out int pid);

                double.TryParse(
                    parts[4],
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out double cpu
                );

                double.TryParse(parts[5], out double rss);

                return new
                {
                    pid,
                    user = parts[1],
                    state = parts[2].Substring(0, 1),
                    command = parts[3],
                    cpu,
                    memMB = Math.Round(rss / 1024, 1),
                    started = string.Join(" ", parts.Skip(6))
                };
            })
            .Where(p => p != null)
            .OrderByDescending(p => p!.cpu)
            .Take(200)
            .Cast<object>()
            .ToList();

        return processes;
    }
}