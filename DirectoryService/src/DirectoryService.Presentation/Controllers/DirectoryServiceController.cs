using DirectoryService.Application.DirectoryServiceManagement.Departments.Create;
using DirectoryService.Application.DirectoryServiceManagement.Departments.Update;
using DirectoryService.Application.DirectoryServiceManagement.Departments.UpdateLocations;
using DirectoryService.Application.DirectoryServiceManagement.DTOs;
using DirectoryService.Application.DirectoryServiceManagement.Locations.Create;
using DirectoryService.Application.DirectoryServiceManagement.Locations.LinkDepartmentAndLocation;
using DirectoryService.Application.DirectoryServiceManagement.Locations.Update;
using DirectoryService.Application.DirectoryServiceManagement.Positions.Create;
using DirectoryService.Presentation.EndpointResults;
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
    public async Task<EndpointResult<Guid>> CreateLocation(
        [FromServices] CreateLocationHandler handler,
        [FromBody] CreateLocationDto request,
        CancellationToken cancellationToken = default)
    {
        return await handler.Handle(request, cancellationToken);
    }
    
    [HttpPatch("/api/locations/{id:guid}")]
    public async Task<EndpointResult<Guid>> UpdateLocation(
        [FromServices] UpdateLocationHandler handler,
        [FromBody] UpdateLocationDto request,
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = request with { LocationId = id };
        return await handler.Handle(command, cancellationToken);
    }
    
    [HttpPost("/api/departments/{departmentId:guid}/locations/{locationId:guid}")]
    public async Task<EndpointResult<Guid>> LinkDepartmentAndLocation(
        [FromServices] LinkDepartmentAndLocationHandler handler,
        [FromRoute] Guid departmentId,
        [FromRoute] Guid locationId,
        CancellationToken cancellationToken = default)
    {
        var command = new LinkDepartmentAndLocationDto(departmentId, locationId);
        return await handler.Handle(command, cancellationToken);
    }

    [HttpPost("/api/departments")]
    public async Task<EndpointResult<Guid>> CreateDepartment(
        [FromServices] CreateDepartmentHandler handler,
        [FromBody] CreateDepartmentDto request, 
        CancellationToken cancellationToken = default)
    {
        return await handler.Handle(request, cancellationToken);
    }
    
    [HttpPatch("/api/departments/{id:guid}")]
    public async Task<EndpointResult<Guid>> UpdateDepartment(
        [FromServices] UpdateDepartmentHandler handler,
        [FromBody] UpdateDepartmentDto request, 
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = request with { DepartmentId = id };
        return await handler.Handle(command, cancellationToken);
    }
    
    [HttpPost("/api/positions")]
    public async Task<EndpointResult<Guid>> CreatePosition(
        [FromServices] CreatePositionHandler handler,
        [FromBody] CreatePositionDto request, 
        CancellationToken cancellationToken = default)
    {
        return await handler.Handle(request, cancellationToken);
    }
    
    [HttpPut("/api/departments/{departmentId}/locations")]
    public async Task<EndpointResult<Guid>> UpdateLocations(
        [FromServices] UpdateLocationsByDepartmentHandler handler,
        [FromBody] UpdateLocationsByDepartmentDto request, 
        CancellationToken cancellationToken = default)
    {
        return await handler.Handle(request, cancellationToken);
    }
    
}