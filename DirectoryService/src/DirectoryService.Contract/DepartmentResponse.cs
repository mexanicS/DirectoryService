namespace DirectoryService.Contract;

public record DepartmentResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Identifier { get; init; } = string.Empty;
    public string Path { get; init; } = string.Empty;
    public int Depth { get; init; }
    public Guid? ParentId { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }

    public DepartmentResponse()
    {
    }

    public DepartmentResponse(
        Guid id,
        string name,
        string identifier,
        string path,
        int depth,
        Guid? parentId,
        bool isActive,
        DateTime createdAt,
        DateTime? updatedAt)
    {
        Id = id;
        Name = name;
        Identifier = identifier;
        Path = path;
        Depth = depth;
        ParentId = parentId;
        IsActive = isActive;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }
}
