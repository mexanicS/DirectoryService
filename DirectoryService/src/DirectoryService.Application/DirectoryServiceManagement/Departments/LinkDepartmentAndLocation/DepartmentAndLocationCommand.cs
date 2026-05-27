namespace DirectoryService.Application.DirectoryServiceManagement.Departments.LinkDepartmentAndLocation;

public record DepartmentAndLocationCommand(
    Guid DepartmentId,
    Guid LocationId);