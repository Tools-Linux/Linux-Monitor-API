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
        var uptimesecond = double.Parse(
            content.Split(' ')[0], CultureInfo.InvariantCulture);

        var uptime = TimeSpan.FromSeconds(uptimesecond);

        return Ok(new
        {
            time = uptime
        });
    }
}