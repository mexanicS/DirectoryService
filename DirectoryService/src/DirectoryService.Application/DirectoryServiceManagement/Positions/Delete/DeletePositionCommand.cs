using System;

namespace DirectoryService.Application.DirectoryServiceManagement.Positions.Delete;

public record DeletePositionCommand(
    Guid PositionId);