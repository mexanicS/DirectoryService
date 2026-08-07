namespace DirectoryService.Contract;

public record DepartmentTreeResponse
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
    public List<DepartmentTreeResponse> Children { get; set; } = [];

    public DepartmentTreeResponse()
    {
    }

    public DepartmentTreeResponse(
        Guid id,
        string name,
        string identifier,
        string path,
        int depth,
        Guid? parentId,
        bool isActive,
        DateTime createdAt,
        DateTime? updatedAt,
        List<DepartmentTreeResponse>? children = null)
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
        Children = children ?? [];
    }
}