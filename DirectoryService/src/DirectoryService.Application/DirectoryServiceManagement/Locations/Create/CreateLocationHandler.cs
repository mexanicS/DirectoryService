using CSharpFunctionalExtensions;
using DirectoryService.Application.DirectoryServiceManagement.DTOs;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Locations;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace DirectoryService.Application.DirectoryServiceManagement.Locations.Create;

public class CreateLocationHandler(
    ILocationsRepository locationsRepository,
    ILogger<CreateLocationHandler> logger,
    IValidator<CreateLocationDto> validator)
{
    public async Task<Result<Guid, Errors>> Handle(CreateLocationDto createLocationDto, 
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(createLocationDto, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }
        
        var locationCreateResult = CreateLocation(createLocationDto);
        
        var existsByAddress = await locationsRepository
            .ExistsActiveLocationByAddressAsync(locationCreateResult.Value.Address, cancellationToken);
        
        if (existsByAddress.Value)
            return GeneralErrors.AlreadyExistByAddress().ToErrors();

        var addAsync = await locationsRepository.AddAsync(locationCreateResult.Value, cancellationToken);
        
        if (addAsync.IsFailure)
        {
            return new Errors([addAsync.Error]);
        }
        
        logger.LogInformation("Created location added with id {locationId}", locationCreateResult.Value.Id.Value);
        
        return locationCreateResult.Value.Id.Value;
    }
    
    private Result<Location> CreateLocation(CreateLocationDto createLocationDto)
    {
        var locationId = new LocationId(Guid.NewGuid());

        var locationName = LocationName.Create(createLocationDto.LocationName);
        
        var address = Address.Create(
            createLocationDto.Address.City, 
            createLocationDto.Address.Street,
            createLocationDto.Address.HouseNumber, 
            createLocationDto.Address.ZipCode);
       
        var timezone = Timezone.Create(createLocationDto.Timezone);
        
        var location = Location.Create(locationId,
            locationName.Value,
            address.Value,
            timezone.Value);
        
        return location.Value;
    }
}