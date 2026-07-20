using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;

namespace Linux_Monitor_API.Controllers.Disk;

[ApiController]
[Route("api/disk")]
public class DiskController : ControllerBase
{

    [HttpGet]
    public IActionResult Get()
    {
        var drive = new DriveInfo("/");

        if (!drive.IsReady)
        {
            return StatusCode(500, "Le disque n'est pas disponible");
        }

        long total = drive.TotalSize;
        long free = drive.AvailableFreeSpace;
        long used = total - free;
        
        return Ok(new
        {
            diskname = drive,
            totalGb = Math.Round(total / 1024d / 1024 / 1024, 2),
            usedGb = Math.Round(used / 1024d / 1024 / 1024, 2),
            freeGb = Math.Round(free / 1024d / 1024 / 1024, 2),
            usage = Math.Round((double)used / total * 100, 2)
        });
    }
}