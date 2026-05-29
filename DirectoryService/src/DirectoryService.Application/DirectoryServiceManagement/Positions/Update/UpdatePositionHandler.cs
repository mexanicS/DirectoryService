using CSharpFunctionalExtensions;
using DirectoryService.Application.DataBase;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Positions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace DirectoryService.Application.DirectoryServiceManagement.Positions.Update;

public class UpdatePositionHandler
{
    private readonly IPositionsRepository _positionsRepository;
    private readonly ILogger<UpdatePositionHandler> _logger;
    private readonly IValidator<UpdatePositionCommand> _validator;
    private readonly ITransactionManager _transactionManager;

    public UpdatePositionHandler(IPositionsRepository positionsRepository,
        ILogger<UpdatePositionHandler> logger,
        IValidator<UpdatePositionCommand> validator,
        ITransactionManager transactionManager)
    {
        _positionsRepository = positionsRepository;
        _logger = logger;
        _validator = validator;
        _transactionManager = transactionManager;
    }

    public async Task<Result<Guid, Errors>> Handle(
        UpdatePositionCommand updatePositionCommand, 
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(updatePositionCommand, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }
        var positionName = PositionName.Create(updatePositionCommand.Name).Value;
        var description = Description.Create(updatePositionCommand.Description).Value;
        
        var activePositionByName = await _positionsRepository.IsActivePositionByName(positionName, cancellationToken);
        if (activePositionByName.Value)
        {
            return GeneralErrors.AlreadyExist().ToErrors();
        }
        
        var position = await _positionsRepository.GetById(updatePositionCommand.PositionId, cancellationToken);

        if (position.IsFailure)
        {
            return position.Error.ToErrors();
        }
        
        position.Value.UpdateMainInformation(positionName, description);

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);

        if (saveResult.IsFailure)
        {
            return saveResult.Error.ToErrors();
        }
        
        _logger.LogInformation("Position updated with id={Id}", position.Value.Id.Value);

        return position.Value.Id.Value;
    }
}