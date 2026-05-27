using CSharpFunctionalExtensions;
using DirectoryService.Application.DataBase;
using DirectoryService.Application.DirectoryServiceManagement.DTOs;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Locations;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace DirectoryService.Application.DirectoryServiceManagement.Locations.Update;

public class UpdateLocationHandler
{
    private readonly ILocationsRepository _locationsRepository;
    private readonly ILogger<UpdateLocationHandler> _logger;
    private readonly IValidator<UpdateLocationDto> _validator;
    private readonly ITransactionManager _transactionManager;

    public UpdateLocationHandler(ILocationsRepository locationsRepository,
        ILogger<UpdateLocationHandler> logger,
        IValidator<UpdateLocationDto> validator,
        ITransactionManager transactionManager)
    {
        _locationsRepository = locationsRepository;
        _logger = logger;
        _validator = validator;
        _transactionManager = transactionManager;
    }

    public async Task<Result<Guid, Errors>> Handle(UpdateLocationDto updateLocationDto, 
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(updateLocationDto, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }
        
        var locationCreateResult = CreateLocation(updateLocationDto);
        
        var existsByAddress = await _locationsRepository
            .ExistsActiveLocationByAddressAsync(locationCreateResult.Value.Address, cancellationToken);

        if (existsByAddress.Value)
        {
            return GeneralErrors.AlreadyExistByAddress().ToErrors();
        }
        
        var locationResult = await _locationsRepository.GetById(locationCreateResult.Value.Id.Value, cancellationToken);
        if (locationResult.IsFailure)
        {
            return locationResult.Error.ToErrors();
        }
        
        locationResult.Value.UpdateMainInformation(locationCreateResult.Value.Name, 
            locationCreateResult.Value.Address,  
            locationCreateResult.Value.Timezone);
        
        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);

        if (saveResult.IsFailure)
        {
            return saveResult.Error.ToErrors();
        }
        
        _logger.LogInformation("Updated location with id {locationId}", locationCreateResult.Value.Id.Value);
        
        return locationCreateResult.Value.Id.Value;
    }
    
    private Result<Location> CreateLocation(UpdateLocationDto createLocationDto)
    {
        var id = new LocationId(createLocationDto.LocationId);
        
        var locationName = LocationName.Create(createLocationDto.LocationName);
        
        var address = Address.Create(
            createLocationDto.Address.City, 
            createLocationDto.Address.Street,
            createLocationDto.Address.HouseNumber, 
            createLocationDto.Address.ZipCode);
       
        var timezone = Timezone.Create(createLocationDto.Timezone);
        
        var location = Location.Create(id,
            locationName.Value,
            address.Value,
            timezone.Value);
        
        return location.Value;
    }
}