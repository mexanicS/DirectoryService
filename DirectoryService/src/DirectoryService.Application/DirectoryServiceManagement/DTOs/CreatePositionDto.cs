namespace DirectoryService.Application.DirectoryServiceManagement.DTOs;

public record CreatePositionDto(string Name,
    string Description,
    Guid[] DepartmentIds);