namespace DirectoryService.Application.DirectoryServiceManagement.Departments.Move;

public sealed record MoveDepartmentCommand(Guid DepartmentId, Guid? ParentId);
