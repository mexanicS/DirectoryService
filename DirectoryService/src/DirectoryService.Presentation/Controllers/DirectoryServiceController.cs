using DirectoryService.Application;
using DirectoryService.Application.DirectoryServiceManagement.Commands.Locations;
using DirectoryService.Application.DirectoryServiceManagement.DTOs;
using DirectoryService.Presentation.EndpointResults;
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
    public async Task<EndpointResult<Guid>> Create(
        [FromServices] CreateLocationHandler handler,
        [FromBody] CreateLocationDto request,
        CancellationToken cancellationToken = default)
    {
        return await handler.Handle(request, cancellationToken);
    }
}