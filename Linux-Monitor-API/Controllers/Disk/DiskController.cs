using Linux_Monitor_API.Services.Disk;
using Microsoft.AspNetCore.Mvc;

namespace Linux_Monitor_API.Controllers.Disk;

[ApiController]
[Route("api/disk")]
public class DiskController : ControllerBase
{
    
    public readonly DiskServices _disk;
    
    public DiskController(DiskServices disk)
    {
        _disk = disk;
    }
    
    [HttpGet]
    public IActionResult Get()
    {
        var disk = _disk.Get();
        return Ok(disk);
    }
}
