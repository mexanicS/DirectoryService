namespace DirectoryService.Application.DirectoryServiceManagement.DTOs;

public record UpdateLocationDto(
    Guid LocationId,
    string LocationName,
    AddressDto Address,
    string Timezone);