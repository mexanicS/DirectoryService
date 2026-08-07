namespace DirectoryService.Contract;

public record DepartmentWithChildren(
    Guid Id,
    string Name,
    string Path,
    int Depth,
    long ChildrenCount,
    bool HasChildren);