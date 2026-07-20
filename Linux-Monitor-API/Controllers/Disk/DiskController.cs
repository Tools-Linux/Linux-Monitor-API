using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Linux_Monitor_API.Controllers.Disk;

[ApiController]
[Route("api/disk")]
public class DiskController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var psi = new ProcessStartInfo
        {
            FileName = "lsblk",
            Arguments = "-J -o NAME,SIZE,TYPE,MODEL,SERIAL,MOUNTPOINT",
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        using var process = Process.Start(psi);

        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        var data = JsonSerializer.Deserialize<object>(output);

        return Ok(data);
    }
}