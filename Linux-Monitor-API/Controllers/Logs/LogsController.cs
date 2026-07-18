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
            .Select(line => JsonSerializer.Deserialize<JsonElement>(line))
            .ToList();


        return Ok(new
        {
            logs
        });
    }
}