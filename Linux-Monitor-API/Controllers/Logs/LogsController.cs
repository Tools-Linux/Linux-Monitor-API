using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Linux_Monitor_API.Controllers.Logs;

[ApiController]
[Route("api/logs")]
public class LogsController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var process = Process.Start(new ProcessStartInfo
        {
            FileName = "journalctl",
            Arguments = "-n 100 --no-pager --output=json",
            RedirectStandardOutput = true,
            UseShellExecute = false
        });

        string output = process!.StandardOutput.ReadToEnd();
        process.WaitForExit();

        var logs = output
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line =>
            {
                var json = JsonDocument.Parse(line).RootElement;

                return new
                {
                    timestamp = json.TryGetProperty("__REALTIME_TIMESTAMP", out var time)
                        ? DateTimeOffset.FromUnixTimeMilliseconds(
                            long.Parse(time.GetString()!) / 1000
                        )
                        : DateTimeOffset.Now,

                    level = json.TryGetProperty("PRIORITY", out var priority)
                        ? priority.GetString() switch
                        {
                            "0" or "1" or "2" or "3" => "ERROR",
                            "4" => "WARNING",
                            "5" or "6" => "INFO",
                            _ => "DEBUG"
                        }
                        : "UNKNOWN",

                    service = json.TryGetProperty("_SYSTEMD_UNIT", out var unit)
                        ? unit.GetString()
                        : "system",

                    pid = json.TryGetProperty("_PID", out var pid)
                        ? pid.GetString()
                        : null,

                    message = json.TryGetProperty("MESSAGE", out var msg)
                        ? msg.GetString()
                        : ""
                };
            })
            .ToList();
        
        return Ok(new
        {
            logs
        });
    }
}