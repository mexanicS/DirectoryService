using DirectoryService.Application.DirectoryServiceManagement.Departments.AddPositionToDepartment;
using DirectoryService.Application.DirectoryServiceManagement.Departments.Create;
using DirectoryService.Application.DirectoryServiceManagement.Departments.Delete;
using DirectoryService.Application.DirectoryServiceManagement.Departments.GetById;
using DirectoryService.Application.DirectoryServiceManagement.Departments.LinkDepartmentAndLocation;
using DirectoryService.Application.DirectoryServiceManagement.Departments.RemovePositionFromDepartment;
using DirectoryService.Application.DirectoryServiceManagement.Departments.UnLinkDepartmentAndLocationHandler;
using DirectoryService.Application.DirectoryServiceManagement.Departments.Update;
using DirectoryService.Application.DirectoryServiceManagement.DTOs;
using DirectoryService.Contract;
using DirectoryService.Presentation.EndpointResults;
using DirectoryService.Presentation.Requests;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Controllers.DepartmentsController;

[ApiController]
[Route("[controller]")]
public class DepartmentController : ControllerBase
{
    public DepartmentController()
    {
    }
    
    [HttpGet("/api/departments/{departmentId:guid}")]
    public async Task<EndpointResult<DepartmentResponse>> GetDepartment(
        [FromServices] GetDepartmentByIdHandler handler,
        [FromRoute] Guid departmentId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetDepartmentByIdQuery(departmentId);
        return await handler.Handle(query, cancellationToken);
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
    
    [HttpDelete("/api/departments/{departmentId:guid}")]
    public async Task<EndpointResult<Guid>> DeleteDepartment(
        [FromServices] DeleteDepartmentHandler handler,
        [FromRoute] Guid departmentId,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteDepartmentCommand(departmentId);
        
        return await handler.Handle(command, cancellationToken);
    }
    
    [HttpPost("/api/departments/{departmentId:guid}/positions/{positionId:guid}")]
    public async Task<EndpointResult<Guid>> AddPositionToDepartment(
        [FromServices] AddPositionToDepartmentHandler handler,
        [FromRoute] Guid departmentId,
        [FromRoute] Guid positionId,
        CancellationToken cancellationToken = default)
    {
        var command = new AddPositionToDepartmentCommand(departmentId, positionId);
    
        return await handler.Handle(command, cancellationToken);
    }
    
    [HttpDelete("/api/departments/{departmentId:guid}/positions/{positionId:guid}")]
    public async Task<EndpointResult<Guid>> RemovePositionFromDepartment(
        [FromServices] RemovePositionFromDepartmentHandler handler,
        [FromRoute] Guid departmentId,
        [FromRoute] Guid positionId,
        CancellationToken cancellationToken = default)
    {
        var command = new RemovePositionFromDepartmentCommand(departmentId, positionId);
    
        return await handler.Handle(command, cancellationToken);
    }
}