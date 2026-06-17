namespace DirectoryService.Contract;

public record LocationDto(Guid Id,
    string Name,
    string City,
    string Street,
    string HouseNumber,
    string? ZipCode,
    int DepartmentCount);
    
public record TopLocationResponse(IReadOnlyList<LocationDto> Locations);