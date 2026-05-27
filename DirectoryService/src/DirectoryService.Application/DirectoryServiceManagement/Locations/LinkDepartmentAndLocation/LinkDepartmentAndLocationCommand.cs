namespace DirectoryService.Application.DirectoryServiceManagement.Locations.LinkDepartmentAndLocation;

public record LinkDepartmentAndLocationCommand(
    Guid DepartmentId,
    Guid LocationId);