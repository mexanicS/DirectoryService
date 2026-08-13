namespace DirectoryService.Application.DirectoryServiceManagement.Departments.Move;

public sealed record MoveDepartmentResponse(
    Guid Id,
    Guid? ParentId,
    string Path,
    int Depth,
    DateTime? UpdatedAt);
