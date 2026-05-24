namespace DirectoryService.Application.DirectoryServiceManagement.DTOs;

public record MoveDepartmentsDto(Guid DepartmentId,
    Guid? ParentDepartmentId);