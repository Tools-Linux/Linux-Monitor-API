using System.Globalization;
using Microsoft.AspNetCore.Mvc;

namespace Linux_Monitor_API.Services.Information;

public class InformationServices
{
    public async Task<object> Get()
    {
        var content = await System.IO.File.ReadAllTextAsync("/proc/uptime");
        var uptimeSeconds = double.Parse(
            content.Split(' ')[0],
            CultureInfo.InvariantCulture);

        var uptime = TimeSpan.FromSeconds(uptimeSeconds);

        var formattedUptime = $"{uptime.Days:00}:{uptime.Hours:00}:{uptime.Minutes:00}:{uptime.Seconds:00}";

        return (new
        {
            time = formattedUptime
        });
    }
}