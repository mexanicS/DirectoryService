namespace DirectoryService.Application.DirectoryServiceManagement.Positions.Update;

public record UpdatePositionCommand(
    Guid PositionId,
    string Name,
    string Description);