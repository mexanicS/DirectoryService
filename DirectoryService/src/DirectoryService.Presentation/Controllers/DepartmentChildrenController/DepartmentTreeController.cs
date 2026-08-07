using DirectoryService.Application.DirectoryServiceManagement.Departments.GetById;
using DirectoryService.Application.DirectoryServiceManagement.DepartmentTree.GetAncestorsByPath;
using DirectoryService.Application.DirectoryServiceManagement.DepartmentTree.GetDirectChildrenByNode;
using DirectoryService.Application.DirectoryServiceManagement.DepartmentTree.GetNodesByName;
using DirectoryService.Application.DirectoryServiceManagement.DepartmentTree.GetRootNodeDepartment;
using DirectoryService.Contract;
using DirectoryService.Presentation.EndpointResults;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Controllers.DepartmentChildrenController;

[ApiController]
[Route("[controller]")]
public class DepartmentTreeController : ControllerBase
{
    public DepartmentTreeController()
    {
    }
    
    [HttpGet("/api/departments/tree")]
    public async Task<EndpointResult<List<DepartmentWithChildren>>> GetRootNodeDepartment(
        [FromServices] GetRootNodeDepartmentHandler handler,
        CancellationToken cancellationToken = default)
    {
        return await handler.Handle(cancellationToken);
    }
    
    [HttpGet("/api/departments/{path}/children")]
    public async Task<EndpointResult<List<DepartmentWithChildren>>> GetDirectChildrenByNodePath(
        [FromRoute] string path, 
        [FromServices] GetDirectChildrenByNodeHandler handler,
        CancellationToken cancellationToken = default)
    {
        return await handler.Handle(path, cancellationToken);
    }
    
    [HttpGet("/departments/{path}/ancestors")]
    public async Task<EndpointResult<List<DepartmentResponse>>> GetAncestorsByNodePath(
        [FromRoute] string path, 
        [FromServices] GetAncestorsByPathHandler handler,
        CancellationToken cancellationToken = default)
    {
        return await handler.Handle(path, cancellationToken);
    }
    
    [HttpGet("/departments/tree/search")]
    public async Task<EndpointResult<List<DepartmentTreeResponse>>> GetNodesBySearch(
        [FromQuery] string? name,
        [FromServices] GetNodesByNameHandler handler,
        CancellationToken cancellationToken = default)
    {
        var command = new GetNodesByNameHandlerCommand(name);

        return await handler.Handle(command, cancellationToken);
    }
}