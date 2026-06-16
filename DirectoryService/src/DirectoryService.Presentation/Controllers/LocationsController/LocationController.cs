using DirectoryService.Application.DirectoryServiceManagement.Departments.UpdateLocations;
using DirectoryService.Application.DirectoryServiceManagement.DTOs;
using DirectoryService.Application.DirectoryServiceManagement.Locations.Create;
using DirectoryService.Application.DirectoryServiceManagement.Locations.Delete;
using DirectoryService.Application.DirectoryServiceManagement.Locations.GetById;
using DirectoryService.Application.DirectoryServiceManagement.Locations.GetTop;
using DirectoryService.Application.DirectoryServiceManagement.Locations.Update;
using DirectoryService.Contract;
using DirectoryService.Presentation.EndpointResults;
using DirectoryService.Presentation.Requests;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Controllers.LocationsController;

[ApiController]
[Route("[controller]")]
public class LocationController : ControllerBase
{
    public LocationController()
    {
    }

    [HttpGet("/api/locations/{locationId:guid}")]
    public async Task<EndpointResult<LocationResponse>> GetLocation(
        [FromServices] GetLocationByIdHandler handler,
        [FromRoute] Guid locationId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetLocationByIdQuery(locationId);
        return await handler.Handle(query, cancellationToken);
    }
    
    [HttpGet("/api/locations/top")]
    public async Task<EndpointResult<TopLocationResponse>> GetTopLocationByCountDepartments(
        [FromServices] GetTopLocationByCountDepartmentsHandler handler,
        CancellationToken cancellationToken = default)
    {
        return await handler.Handle(cancellationToken);
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
    
    [HttpDelete("/api/locations/{locationId:guid}")]
    public async Task<EndpointResult<Guid>> DeleteLocation(
        [FromServices] DeleteLocationHandler handler,
        [FromRoute] Guid locationId,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteLocationCommand(locationId); 
    
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