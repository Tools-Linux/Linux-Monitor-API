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
            Arguments = "list-units --type=service --state=running --no-pager --no-legend",
            RedirectStandardOutput = true,
            UseShellExecute = false
        });

        string output = process!.StandardOutput.ReadToEnd();
        process.WaitForExit();

        var services = output
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0])
            .ToList();

        int serviceCount = services.Count;
        
        return Ok(new
        {
            serviceCount,
            services
        });
    }
}