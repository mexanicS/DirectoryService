using System;

namespace DirectoryService.Application.DirectoryServiceManagement.Locations.Delete;

public record DeleteLocationCommand(
    Guid LocationId);