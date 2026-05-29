using DirectoryService.Application.DirectoryServiceManagement.DTOs;

namespace DirectoryService.Presentation.Requests;

public record UpdateLocationRequest(
    string LocationName,
    AddressDto Address,
    string Timezone);