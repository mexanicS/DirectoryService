namespace DirectoryService.Presentation.Requests;

public record UpdateLocationsByDepartmentRequest(
    Guid[] LocationIds);