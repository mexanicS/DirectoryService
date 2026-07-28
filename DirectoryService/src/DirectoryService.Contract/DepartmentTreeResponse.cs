namespace DirectoryService.Contract;

public record DepartmentTreeResponse(
    Guid Id,
    string Name,
    string Identifier,
    string Path,
    int Depth,
    Guid? ParentId,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<DepartmentTreeResponse>  Children);