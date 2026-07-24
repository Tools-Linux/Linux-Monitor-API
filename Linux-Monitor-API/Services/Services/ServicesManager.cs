using System.Diagnostics;

namespace Linux_Monitor_API.Services.Services;

public class ServicesManager
{
    public async Task<List<object>> GetServices()
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "systemctl",
                Arguments = "list-units --type=service --all --no-legend --no-pager",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();

        string output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();

        var services = output
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line =>
            {
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length < 5)
                    return null;

                return new
                {
                    name = parts[0],
                    state = parts[2],
                    sub = parts[3]
                };
            })
            .Where(x => x != null)
            .Cast<object>()
            .ToList();

        return services;
    }
}