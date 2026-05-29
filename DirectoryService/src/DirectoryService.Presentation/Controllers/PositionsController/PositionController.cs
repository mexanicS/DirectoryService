using DirectoryService.Application.DirectoryServiceManagement.DTOs;
using DirectoryService.Application.DirectoryServiceManagement.Positions.Create;
using DirectoryService.Application.DirectoryServiceManagement.Positions.Delete;
using DirectoryService.Application.DirectoryServiceManagement.Positions.Update;
using DirectoryService.Presentation.EndpointResults;
using DirectoryService.Presentation.Requests;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Controllers.PositionsController;

[ApiController]
[Route("[controller]")]
public class PositionController : ControllerBase
{
    public PositionController()
    {
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
    
    [HttpDelete("/api/positions/{positionId:guid}")]
    public async Task<EndpointResult<Guid>> DeletePosition(
        [FromServices] DeletePositionHandler handler,
        [FromRoute] Guid positionId, 
        CancellationToken cancellationToken = default)
    {
        var command = new DeletePositionCommand(positionId);
    
        return await handler.Handle(command, cancellationToken);
    }
}