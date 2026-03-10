using System;
using System.Threading;
using System.Threading.Tasks;
using DirectoryService.Application.DirectoryServiceManagement.Departments.Create;
using DirectoryService.Application.DirectoryServiceManagement.Departments.UpdateLocations;
using DirectoryService.Application.DirectoryServiceManagement.DTOs;
using DirectoryService.Application.DirectoryServiceManagement.Locations.Create;
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

    [HttpPost("/api/departments")]
    public async Task<EndpointResult<Guid>> CreateDepartment(
        [FromServices] CreateDepartmentHandler handler,
        [FromBody] CreateDepartmentDto request, 
        CancellationToken cancellationToken = default)
    {
        return await handler.Handle(request, cancellationToken);
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