using CSharpFunctionalExtensions;
using DirectoryService.Application.DirectoryServiceManagement.DTOs;
using DirectoryService.Domain.Locations;

namespace DirectoryService.Application.DirectoryServiceManagement.Commands.Locations;

public class CreateLocationHandler
{
    private readonly ILocationsRepository _locationsRepository;

    public CreateLocationHandler(ILocationsRepository locationsRepository)
    {
        _locationsRepository = locationsRepository;
    }

    public async Task<Result<Guid, string>> Handle(CreateLocationDto createLocationDto, 
        CancellationToken cancellationToken)
    {
        var location = CreateLocation(createLocationDto);
        
        if (location.IsFailure)
        {
            return location.Error;
        }
        
        await _locationsRepository.AddAsync(location.Value, cancellationToken);
        
        var saveResult = await _locationsRepository.SaveChangesAsync(cancellationToken);
        
        if (saveResult.IsFailure)
        {
            return saveResult.Error;
        }
        
        return location.Value.Id.Value;
    }
    
    private Result<Location, string> CreateLocation(CreateLocationDto createLocationDto)
    {
        //TO DO перенести проверки vo в валидатор (когда добавлю)
        var locationId = new LocationId(Guid.NewGuid());

        var locationName = LocationName.Create(createLocationDto.LocationName);
        if (locationName.IsFailure)
        {
            return locationName.Error;
        }
        
        var address = Address.Create(
            createLocationDto.Address.City, 
            createLocationDto.Address.Street,
            createLocationDto.Address.HouseNumber, 
            createLocationDto.Address.ZipCode);
        if (address.IsFailure)
        {
            return address.Error;
        }
        
        var timezone = Timezone.Create(createLocationDto.Timezone);
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