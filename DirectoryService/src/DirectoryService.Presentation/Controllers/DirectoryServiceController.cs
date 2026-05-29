using DirectoryService.Application.DirectoryServiceManagement.Departments.Create;
using DirectoryService.Application.DirectoryServiceManagement.Departments.LinkDepartmentAndLocation;
using DirectoryService.Application.DirectoryServiceManagement.Departments.UnLinkDepartmentAndLocationHandler;
using DirectoryService.Application.DirectoryServiceManagement.Departments.Update;
using DirectoryService.Application.DirectoryServiceManagement.Departments.UpdateLocations;
using DirectoryService.Application.DirectoryServiceManagement.DTOs;
using DirectoryService.Application.DirectoryServiceManagement.Locations.Create;
using DirectoryService.Application.DirectoryServiceManagement.Locations.Update;
using DirectoryService.Application.DirectoryServiceManagement.Positions.Create;
using DirectoryService.Application.DirectoryServiceManagement.Positions.Update;
using DirectoryService.Presentation.EndpointResults;
using DirectoryService.Presentation.Requests;
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
    
    [HttpPatch("/api/locations/{locationId:guid}")]
    public async Task<EndpointResult<Guid>> UpdateLocation(
        [FromServices] UpdateLocationHandler handler,
        [FromBody] UpdateLocationRequest request,
        [FromRoute] Guid locationId,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateLocationCommand(locationId, request.LocationName, request.Address, request.Timezone); 
        
        return await handler.Handle(command, cancellationToken);
    }
    
    [HttpPost("/api/departments/{departmentId:guid}/locations/{locationId:guid}")]
    public async Task<EndpointResult<Guid>> LinkDepartmentAndLocation(
        [FromServices] LinkDepartmentAndLocationHandler handler,
        [FromRoute] Guid departmentId,
        [FromRoute] Guid locationId,
        CancellationToken cancellationToken = default)
    {
        var command = new DepartmentAndLocationCommand(departmentId, locationId);
        
        return await handler.Handle(command, cancellationToken);
    }
    
    [HttpDelete("api/departments/{departmentId:guid}/locations/{locationId:guid}")]
    public async Task<EndpointResult<Guid>> UnLinkDepartmentAndLocation(
        [FromServices] UnLinkDepartmentAndLocationHandler handler,
        [FromRoute] Guid departmentId,
        [FromRoute] Guid locationId,
        CancellationToken cancellationToken = default)
    {
        var command = new DepartmentAndLocationCommand(departmentId, locationId);
        
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
    
    [HttpPatch("/api/departments/{departmentId:guid}")]
    public async Task<EndpointResult<Guid>> UpdateDepartment(
        [FromServices] UpdateDepartmentHandler handler,
        [FromBody] UpdateDepartmentRequest request, 
        [FromRoute] Guid departmentId,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateDepartmentCommand(departmentId, request.Name, request.Identifier);
        
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
    
    [HttpPatch("/api/positions/{positionId:guid}")]
    public async Task<EndpointResult<Guid>> UpdatePosition(
        [FromServices] UpdatePositionHandler handler,
        [FromBody] UpdatePositionRequest request, 
        [FromRoute]Guid positionId, 
        CancellationToken cancellationToken = default)
    {
        var command = new UpdatePositionCommand(positionId, request.Name, request.Description);
        
        return await handler.Handle(command, cancellationToken);
    }
    
    [HttpPost("/api/positions/{id}")]
    public async Task<EndpointResult<Guid>> DeletePosition(
        [FromServices] UpdatePositionHandler handler,
        [FromBody] UpdatePositionRequest request, 
        [FromRoute]Guid positionId, 
        CancellationToken cancellationToken = default)
    {
        var command = new UpdatePositionCommand(positionId, request.Name, request.Description);
        
        return await handler.Handle(command, cancellationToken);
    }

    
    [HttpPut("/api/departments/{departmentId:guid}/locations")]
    public async Task<EndpointResult<Guid>> UpdateLocations(
        [FromServices] UpdateLocationsByDepartmentHandler handler,
        [FromBody] UpdateLocationsByDepartmentRequest request,
        [FromRoute] Guid departmentId, 
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateLocationsByDepartmentCommand(departmentId, request.LocationIds);
        
        return await handler.Handle(command, cancellationToken);
    }
    
}