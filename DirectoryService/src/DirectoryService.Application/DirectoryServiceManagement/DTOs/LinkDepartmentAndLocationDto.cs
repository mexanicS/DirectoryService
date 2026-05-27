namespace DirectoryService.Application.DirectoryServiceManagement.DTOs;

public record LinkDepartmentAndLocationDto(
    Guid DepartmentId,
    Guid LocationId);