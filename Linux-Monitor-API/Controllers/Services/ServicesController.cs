using Linux_Monitor_API.Services.Services;
using Microsoft.AspNetCore.Mvc;

namespace Linux_Monitor_API.Controllers.Services;

[ApiController]
[Route("api/processes")]
public class ProcessesController : ControllerBase
{
    private readonly ProcessManager _manager;

    public ProcessesController(ProcessManager manager)
    {
        _manager = manager;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var processes = await _manager.GetProcesses();

        return Ok(new
        {
            processes
        });
    }
}