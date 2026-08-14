namespace DirectoryService.Contract;

public sealed record MoveDepartmentResponse(
    Guid Id,
    Guid? ParentId,
    string Path,
    int Depth,
    DateTime? UpdatedAt);
