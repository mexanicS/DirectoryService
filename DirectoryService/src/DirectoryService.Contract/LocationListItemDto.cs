namespace DirectoryService.Contract;

public record LocationListItemDto(
    Guid Id,
    string Name,
    string Address,
    DateTime CreatedAt,
    int DepartmentCount);
