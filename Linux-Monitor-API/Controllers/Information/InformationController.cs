using System.Globalization;
using Microsoft.AspNetCore.Mvc;

namespace Linux_Monitor_API.Controllers.Information;

[ApiController]
[Route("api/information")]
public class InformationController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var content = await System.IO.File.ReadAllTextAsync("/proc/uptime");
        var uptimeSeconds = double.Parse(
            content.Split(' ')[0],
            CultureInfo.InvariantCulture);

        var uptime = TimeSpan.FromSeconds(uptimeSeconds);

        var formattedUptime = $"{uptime.Days:00}:{uptime.Hours:00}:{uptime.Minutes:00}:{uptime.Seconds:00}";

        return Ok(new
        {
            time = formattedUptime
        });
    }
}