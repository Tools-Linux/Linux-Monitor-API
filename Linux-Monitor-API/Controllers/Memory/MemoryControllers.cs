using Linux_Monitor_API.Services.Memory;
using Microsoft.AspNetCore.Mvc;

namespace Linux_Monitor_API.Controllers.Memory;

[ApiController]
[Route("api/memory")]
public class MemoryControllers : ControllerBase
{
    private readonly MemoryServices memoryservices;
    
    public MemoryControllers(MemoryServices _memoryservices) =>  memoryservices = _memoryservices;
    
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        return Ok(await memoryservices.GetAsync());
    }
}