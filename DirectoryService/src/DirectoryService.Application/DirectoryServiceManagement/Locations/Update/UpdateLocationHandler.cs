using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Locations;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace DirectoryService.Application.DirectoryServiceManagement.Locations.Update;

public class UpdateLocationHandler(
    ILocationsRepository locationsRepository,
    ILogger<UpdateLocationHandler> logger,
    IValidator<UpdateLocationCommand> validator,
    ITransactionManager transactionManager)
{
    public async Task<Result<Guid, Errors>> Handle(UpdateLocationCommand updateLocationCommand, 
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(updateLocationCommand, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        var locationId = new LocationId(updateLocationCommand.LocationId);
        var locationResult = await locationsRepository.GetById(locationId, cancellationToken);
        if (locationResult.IsFailure)
        {
            return locationResult.Error.ToErrors();
        }

        var locationName = LocationName.Create(updateLocationCommand.LocationName).Value;
        var address = Address.Create(
            updateLocationCommand.Address.City,
            updateLocationCommand.Address.Street,
            updateLocationCommand.Address.HouseNumber,
            updateLocationCommand.Address.ZipCode).Value;
        var timezone = Timezone.Create(updateLocationCommand.Timezone).Value;
        
        var existsByAddress = await locationsRepository
            .ExistsActiveLocationByAddressAsync(
                address,
                cancellationToken,
                locationId);

        if (existsByAddress.Value)
        {
            return GeneralErrors.AlreadyExistByAddress().ToErrors();
        }

        locationResult.Value.UpdateMainInformation(locationName, address, timezone);
        
        var saveResult = await transactionManager.SaveChangesAsync(cancellationToken);

        if (saveResult.IsFailure)
        {
            return saveResult.Error.ToErrors();
        }
        
        logger.LogInformation("Updated location with id {LocationId}", locationId.Value);
        
        return locationId.Value;
    }
}
