namespace DirectoryService.Contract;

public record TopLocationResponse(Guid Id,
    string Name,
    string City,
    string Street,
    string HouseNumber,
    string? ZipCode,
    int DepartmentCount);