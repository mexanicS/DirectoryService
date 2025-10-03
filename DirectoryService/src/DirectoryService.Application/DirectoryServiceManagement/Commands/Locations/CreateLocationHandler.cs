using System.Text;
using CSharpFunctionalExtensions;
using DirectoryService.Application.DirectoryServiceManagement.DTOs;
using DirectoryService.Domain.Locations;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace DirectoryService.Application.DirectoryServiceManagement.Commands.Locations;

public class CreateLocationHandler
{
    private readonly ILocationsRepository _locationsRepository;
    private readonly ILogger<CreateLocationHandler> _logger;
    private readonly IValidator<CreateLocationDto> _validator;

    public CreateLocationHandler(ILocationsRepository locationsRepository,
        ILogger<CreateLocationHandler> logger,
        IValidator<CreateLocationDto> validator)
    {
        _locationsRepository = locationsRepository;
        _logger = logger;
        _validator = validator;
    }

    public async Task<Result<Guid, Errors>> Handle(CreateLocationDto createLocationDto, 
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(createLocationDto, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }
        
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
        
        _logger.LogInformation("Created location added with id {locationId}", locationCreateResult.Value.Id.Value);
        
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