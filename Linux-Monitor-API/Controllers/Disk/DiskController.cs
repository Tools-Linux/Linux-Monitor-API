using Microsoft.AspNetCore.Mvc;

namespace Linux_Monitor_API.Controllers.Disk;

[ApiController]
[Route("api/disk")]
public class DiskController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var drives = DriveInfo.GetDrives();

        var disks = new List<object>();

        foreach (var drive in drives)
        {
            try
            {
                if (!drive.IsReady)
                    continue;

                long total = drive.TotalSize;
                long free = drive.AvailableFreeSpace;
                long used = total - free;

                disks.Add(new
                {
                    name = drive.Name,
                    format = drive.DriveFormat,
                    type = drive.DriveType.ToString(),

                    totalGb = Math.Round(total / 1024d / 1024 / 1024, 2),
                    usedGb = Math.Round(used / 1024d / 1024 / 1024, 2),
                    freeGb = Math.Round(free / 1024d / 1024 / 1024, 2),

                    usage = Math.Round((double)used / total * 100, 2)
                });
            }
            catch
            {
            }
        }

        return Ok(disks);
    }
}