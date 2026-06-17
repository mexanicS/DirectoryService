using CSharpFunctionalExtensions;
using DirectoryService.Application.DataBase;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Locations;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace DirectoryService.Application.DirectoryServiceManagement.Locations.Delete;

public class DeleteLocationHandler(
    ILocationsRepository locationsRepository,
    ILogger<DeleteLocationHandler> logger,
    IValidator<DeleteLocationCommand> validator,
    ITransactionManager transactionManager)
{
    public async Task<Result<Guid, Errors>> Handle(DeleteLocationCommand deleteLocationCommand, 
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(deleteLocationCommand, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }
        
        var locationId = new LocationId(deleteLocationCommand.LocationId);

        var locationResult = await locationsRepository.GetById(locationId, cancellationToken);
        if (locationResult.IsFailure)
        {
            return locationResult.Error.ToErrors();
        }

        locationsRepository.Delete(locationResult.Value);

        var saveResult = await transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            return saveResult.Error.ToErrors();
        }
        
        logger.LogInformation("Deleted location with id {locationId}", locationId.Value);
        
        return locationId.Value;
    }
}