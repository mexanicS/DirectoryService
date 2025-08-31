using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Controllers;

[ApiController]
[Route("[controller]")]
public class DirectoryServiceController : ControllerBase
{

    public DirectoryServiceController()
    {
    }

    public string Get()
    {
        return string.Empty;
    }
}