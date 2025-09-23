using CSharpFunctionalExtensions;
using DirectoryService.Application.DirectoryServiceManagement.DTOs;
using DirectoryService.Domain.Locations;
using SharedKernel;

namespace DirectoryService.Application.DirectoryServiceManagement.Commands.Locations;

public class CreateLocationHandler
{
    private readonly ILocationsRepository _locationsRepository;

    public CreateLocationHandler(ILocationsRepository locationsRepository)
    {
        _locationsRepository = locationsRepository;
    }

    public async Task<Result<Guid, Errors>> Handle(CreateLocationDto createLocationDto, 
        CancellationToken cancellationToken)
    {
        var locationCreateResult = CreateLocation(createLocationDto);
        
        if (locationCreateResult.IsFailure)
        {
            return locationCreateResult.Error;
        }
        
        await _locationsRepository.AddAsync(locationCreateResult.Value, cancellationToken);
        
        var saveResult = await _locationsRepository.SaveChangesAsync(cancellationToken);
        
        if (saveResult.IsFailure)
        {
            return new Errors([GeneralErrors.Failure(saveResult.Error)]);
        }
        
        return locationCreateResult.Value.Id.Value;
    }
    
    private Result<Location, Errors> CreateLocation(CreateLocationDto createLocationDto)
    {
        //TO DO перенести проверки vo в валидатор (когда добавлю)
        var locationId = new LocationId(Guid.NewGuid());

        var locationName = LocationName.Create(createLocationDto.LocationName);
        if (locationName.IsFailure)
        {
            return locationName.Error.ToErrors();
        }
        
        var address = Address.Create(
            createLocationDto.Address.City, 
            createLocationDto.Address.Street,
            createLocationDto.Address.HouseNumber, 
            createLocationDto.Address.ZipCode);
        if (address.IsFailure)
        {
            return address.Error.ToErrors();
        }
        
        var timezone = Timezone.Create(createLocationDto.Timezone);
        if (timezone.IsFailure)
        {
            return timezone.Error.ToErrors();
        }
        
        var location = Location.Create(locationId,
            locationName.Value,
            address.Value,
            timezone.Value);
        
        return location.Value;
    }
}