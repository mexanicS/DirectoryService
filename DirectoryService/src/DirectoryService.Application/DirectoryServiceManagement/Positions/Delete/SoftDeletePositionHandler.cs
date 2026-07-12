using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Positions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace DirectoryService.Application.DirectoryServiceManagement.Positions.Delete;

public class SoftDeletePositionHandler(
    IPositionsRepository positionsRepository,
    ILogger<SoftDeletePositionHandler> logger,
    IValidator<DeletePositionCommand> validator,
    ITransactionManager transactionManager)
{
    public async Task<Result<Guid, Errors>> Handle(
        DeletePositionCommand deletePositionCommand, 
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(deletePositionCommand, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }
        
        var positionId = new PositionId(deletePositionCommand.PositionId);

        var positionResult = await positionsRepository.GetById(positionId, cancellationToken);
        if (positionResult.IsFailure)
        {
            return positionResult.Error.ToErrors();
        }

        positionResult.Value.SoftDelete();

        var saveResult = await transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            return saveResult.Error.ToErrors();
        }
        
        logger.LogInformation("Position soft deleted with id={Id}", positionId.Value);

        return positionId.Value;
    }
}