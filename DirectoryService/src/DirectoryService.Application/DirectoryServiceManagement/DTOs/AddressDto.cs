namespace DirectoryService.Application.DirectoryServiceManagement.DTOs;

public record AddressDto(string City,
    string Street,
    string HouseNumber,
    string? ZipCode);