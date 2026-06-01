namespace DirectoryService.Contract;

public record LocationResponse(
    Guid Id,
    string Name,
    string City,
    string Street,
    string HouseNumber,
    string? ZipCode,
    string Timezone,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
