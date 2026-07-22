using System.Diagnostics;
using System.Text.Json;

namespace Linux_Monitor_API.Services.Logs;

public class LogsServices
{
    public async Task<object> Get(int count = 100)
    {
        var process = Process.Start(new ProcessStartInfo
        {
            FileName = "journalctl",
            Arguments = $"-n {count} --no-pager --output=json",
            RedirectStandardOutput = true,
            UseShellExecute = false
        });

        if (process == null)
            return new { logs = Array.Empty<object>() };
        
        var output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();

        var logs = output
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(ParseLog)
            .Reverse()
            .ToList();


        return new
        {
            count = logs.Count,
            logs
        };
    }
    
    private static object ParseLog(string line)
    {
        try
        {
            using var json = JsonDocument.Parse(line);

            var root = json.RootElement;


            return new
            {
                timestamp = GetTimestamp(root),
                level = GetLevel(root),
                
                service = root.TryGetProperty("_SYSTEMD_UNIT", out var unit)
                    ? unit.GetString()
                    : "system",

                pid = root.TryGetProperty("_PID", out var pid)
                    ? pid.GetString()
                    : null,

                message = root.TryGetProperty("MESSAGE", out var msg)
                    ? msg.GetString()
                    : ""
            };
        }
        catch
        {
            return new
            {
                timestamp = DateTimeOffset.Now,
                level = "UNKNOWN",
                service = "system",
                pid = "",
                message = line
            };
        }
    }
    
    private static DateTimeOffset GetTimestamp(JsonElement json)
    {
        if(!json.TryGetProperty(
            "__REALTIME_TIMESTAMP",
            out var time))
            return DateTimeOffset.Now;
        
        if(long.TryParse(time.GetString(), out var micro))
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(micro / 1000);
        }
        
        return DateTimeOffset.Now;
    }

    private static string GetLevel(JsonElement json)
    {
        if(!json.TryGetProperty("PRIORITY", out var priority)) return "UNKNOWN";

        return priority.GetString() switch
        {
            "0" or "1" or "2" or "3" => "ERROR",
            "4" => "WARNING",
            "5" or "6" => "INFO",
            "7" => "DEBUG",
            _ => "UNKNOWN"
        };
    }
}