using Microsoft.AspNetCore.Mvc;

namespace FileService.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class CommonController : ControllerBase
{
    [HttpGet("/health/live")]
    public IActionResult Live()
    {
        return Ok();
    }
}