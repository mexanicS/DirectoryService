using DirectoryService.Application;
using DirectoryService.Application.DirectoryServiceManagement.Commands.Locations;
using DirectoryService.Application.DirectoryServiceManagement.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Controllers;

[ApiController]
[Route("[controller]")]
public class DirectoryServiceController : ControllerBase
{
    public DirectoryServiceController()
    {
    }

    [HttpPost("/api/locations")]
    public async Task<ActionResult> Create(
        [FromServices] CreateLocationHandler handler,
        [FromBody] CreateLocationDto request,
        CancellationToken cancellationToken = default)
    {
        var result = await handler.Handle(request, cancellationToken);
            
        if(result.IsFailure)
            return BadRequest(result.Error);
            
        return Ok(result.Value);
    }
}