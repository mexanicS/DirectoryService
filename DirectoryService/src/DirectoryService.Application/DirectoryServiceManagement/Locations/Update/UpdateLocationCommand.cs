using DirectoryService.Application.DirectoryServiceManagement.DTOs;

namespace DirectoryService.Application.DirectoryServiceManagement.Locations.Update;

public record UpdateLocationCommand(
    Guid LocationId,
    string LocationName,
    AddressDto Address,
    string Timezone);