using CSharpFunctionalExtensions;
using DirectoryService.Application.DataBase;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Locations;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace DirectoryService.Application.DirectoryServiceManagement.Locations.Delete;

public class DeleteLocationHandler
{
    private readonly ILocationsRepository _locationsRepository;
    private readonly ILogger<DeleteLocationHandler> _logger;
    private readonly IValidator<DeleteLocationCommand> _validator;
    private readonly ITransactionManager _transactionManager;

    public DeleteLocationHandler(ILocationsRepository locationsRepository,
        ILogger<DeleteLocationHandler> logger,
        IValidator<DeleteLocationCommand> validator,
        ITransactionManager transactionManager)
    {
        _locationsRepository = locationsRepository;
        _logger = logger;
        _validator = validator;
        _transactionManager = transactionManager;
    }

    public async Task<Result<Guid, Errors>> Handle(DeleteLocationCommand deleteLocationCommand, 
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(deleteLocationCommand, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }
        
        var locationId = new LocationId(deleteLocationCommand.LocationId);

        var locationResult = await _locationsRepository.GetById(locationId, cancellationToken);
        if (locationResult.IsFailure)
        {
            return locationResult.Error.ToErrors();
        }

        _locationsRepository.Delete(locationResult.Value);

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            return saveResult.Error.ToErrors();
        }
        
        _logger.LogInformation("Deleted location with id {locationId}", locationId.Value);
        
        return locationId.Value;
    }
}