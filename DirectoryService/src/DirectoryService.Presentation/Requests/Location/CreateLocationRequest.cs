using DirectoryService.Application.DirectoryServiceManagement.Commands.Locations;
using DirectoryService.Application.DirectoryServiceManagement.DTOs;

namespace DirectoryService.Presentation.Requests.Location;

public record CreateLocationRequest(
    string LocationName,
    AddressDto Address,
    string Timezone)
{
    public CreateLocationCommand ToCommand()
    {
        return new CreateLocationCommand(LocationName,  Address, Timezone);
    }
}