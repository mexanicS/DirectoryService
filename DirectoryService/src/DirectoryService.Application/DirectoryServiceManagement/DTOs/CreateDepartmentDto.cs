namespace DirectoryService.Application.DirectoryServiceManagement.DTOs;

public record CreateDepartmentDto(string Name,
    string Identifier,
    Guid? ParentId,
    Guid[] LocationIds);