using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Linux_Monitor_API.Controllers.Services;

[ApiController]
[Route("api/services")]
public class ServicesController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var process = Process.Start(new ProcessStartInfo
        {
            FileName = "systemctl",
            Arguments = "list-unit-files --type=service --no-pager --no-legend",
            RedirectStandardOutput = true,
            UseShellExecute = false
        });

        string output = process!.StandardOutput.ReadToEnd();
        process.WaitForExit();

        var services = output
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line =>
            {
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                return new
                {
                    name = parts[0],
                    state = parts.Length > 1 ? parts[1] : "unknown"
                };
            })
            .ToList();

        int totalServices = services.Count;
        int enabledServices = services.Count(s => s.state == "enabled");
        int disabledServices = services.Count(s => s.state == "disabled");
        
        return Ok(new
        {
            services = new
            {
                total = totalServices,
                enabled = enabledServices,
                disabled = disabledServices,
                list = services
            }
        });
    }
}