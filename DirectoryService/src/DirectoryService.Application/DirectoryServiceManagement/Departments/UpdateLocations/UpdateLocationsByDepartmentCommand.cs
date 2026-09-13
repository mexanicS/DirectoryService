namespace DirectoryService.Application.DirectoryServiceManagement.Departments.UpdateLocations;

public record UpdateLocationsByDepartmentCommand(
    Guid DepartmentId, 
    Guid[] LocationIds);