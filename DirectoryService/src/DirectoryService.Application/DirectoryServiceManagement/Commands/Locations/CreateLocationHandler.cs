using CSharpFunctionalExtensions;
using DirectoryService.Domain.Locations;

namespace DirectoryService.Application.DirectoryServiceManagement.Commands.Locations;

public class CreateLocationHandler
{
    private readonly ILocationsRepository _locationsRepository;

    public CreateLocationHandler(ILocationsRepository locationsRepository)
    {
        _locationsRepository = locationsRepository;
    }

    public async Task<Result<Guid, string>> Handle(CreateLocationCommand command, CancellationToken cancellationToken)
    {
        var location = CreateLocation(command);
        
        if (location.IsFailure)
        {
            return location.Error;
        }
        
        await _locationsRepository.AddAsync(location.Value, cancellationToken);
        await _locationsRepository.SaveChangesAsync(cancellationToken);
        
        return location.Value.Id.Value;
    }
    
    private Result<Location, string> CreateLocation(CreateLocationCommand command)
    {
        //TO DO перенести проверки vo в валидатор (когда добавлю)
        var locationId = new LocationId(Guid.NewGuid());

        var locationName = LocationName.Create(command.LocationName);
        if (locationName.IsFailure)
        {
            return locationName.Error;
        }
        
        var address = Address.Create(
            command.Address.City, 
            command.Address.Street,
            command.Address.HouseNumber, 
            command.Address.ZipCode);
        if (address.IsFailure)
        {
            return address.Error;
        }
        
        var timezone = Timezone.Create(command.Timezone);
        if (timezone.IsFailure)
        {
            return timezone.Error;
        }
        
        var location = Location.Create(locationId,
            locationName.Value,
            address.Value,
            timezone.Value);
        
        return location.Value;
    }
}