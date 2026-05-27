namespace DirectoryService.Application.DirectoryServiceManagement.DTOs;

public record UpdateDepartmentDto(
    Guid DepartmentId,
    string Name,
    string Identifier);