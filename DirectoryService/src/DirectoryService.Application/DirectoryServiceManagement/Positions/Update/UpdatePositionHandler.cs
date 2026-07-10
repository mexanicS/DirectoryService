using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Positions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace DirectoryService.Application.DirectoryServiceManagement.Positions.Update;

public class UpdatePositionHandler(
    IPositionsRepository positionsRepository,
    ILogger<UpdatePositionHandler> logger,
    IValidator<UpdatePositionCommand> validator,
    ITransactionManager transactionManager)
{
    public async Task<Result<Guid, Errors>> Handle(
        UpdatePositionCommand updatePositionCommand, 
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(updatePositionCommand, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }
        var positionName = PositionName.Create(updatePositionCommand.Name).Value;
        var description = Description.Create(updatePositionCommand.Description).Value;
        
        var activePositionByName = await positionsRepository.IsActivePositionByName(
            positionName, 
            cancellationToken,
            updatePositionCommand.PositionId);
        
        if (activePositionByName.Value)
        {
            return GeneralErrors.AlreadyExist().ToErrors();
        }
        
        var position = await positionsRepository.GetById(updatePositionCommand.PositionId, cancellationToken);

        if (position.IsFailure)
        {
            return position.Error.ToErrors();
        }
        
        position.Value.UpdateMainInformation(positionName, description);

        var saveResult = await transactionManager.SaveChangesAsync(cancellationToken);

        if (saveResult.IsFailure)
        {
            return saveResult.Error.ToErrors();
        }
        
        logger.LogInformation("Position updated with id={Id}", position.Value.Id.Value);

        return position.Value.Id.Value;
    }
}