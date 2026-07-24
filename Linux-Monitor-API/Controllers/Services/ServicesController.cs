using Linux_Monitor_API.Services.Services;
using Microsoft.AspNetCore.Mvc;

namespace Linux_Monitor_API.Controllers.Services;

[ApiController]
[Route("api/processes")]
public class ProcessesController : ControllerBase
{
    private readonly ServicesManager _manager;

    public ProcessesController(ServicesManager manager)
    {
        _manager = manager;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var processes = await _manager.GetServices();

        return Ok(new
        {
            processes
        });
    }
}