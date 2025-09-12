namespace DirectoryService.Application.DirectoryServiceManagement.DTOs;

public record CreateLocationDto(
    string LocationName,
    AddressDto Address,
    string Timezone);