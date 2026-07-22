using Linux_Monitor_API.Services.Logs;
using Microsoft.AspNetCore.Mvc;

namespace Linux_Monitor_API.Controllers.Logs;

[ApiController]
[Route("api/logs")]
public class LogsController : ControllerBase
{
    private readonly LogsServices _logsServices;
    public LogsController(LogsServices logsServices) => _logsServices = logsServices;

    [HttpGet]
    public IActionResult Get()
    {
        var logs = _logsServices.Get();
        return Ok(logs);
    }
}