namespace DirectoryService.Contract;

public record DepartmentResponse(
    Guid Id,
    string Name,
    string Identifier,
    string Path,
    int Depth,
    Guid? ParentId,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
