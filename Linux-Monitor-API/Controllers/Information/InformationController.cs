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

        var formattedUptime = $"{uptime.Days} jour{(uptime.Days > 1 ? "s" : "")}, " +
                              $"{uptime.Hours} heure{(uptime.Hours > 1 ? "s" : "")}, " +
                              $"{uptime.Minutes} minute{(uptime.Minutes > 1 ? "s" : "")}, " +
                              $"{uptime.Seconds} seconde{(uptime.Seconds > 1 ? "s" : "")}";

        return Ok(new
        {
            time = formattedUptime
        });
    }
}