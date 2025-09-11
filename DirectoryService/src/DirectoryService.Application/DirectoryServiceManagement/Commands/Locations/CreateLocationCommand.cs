using DirectoryService.Application.DirectoryServiceManagement.DTOs;

namespace DirectoryService.Application.DirectoryServiceManagement.Commands.Locations;

public record CreateLocationCommand(string LocationName,
    AddressDto Address,
    string Timezone);