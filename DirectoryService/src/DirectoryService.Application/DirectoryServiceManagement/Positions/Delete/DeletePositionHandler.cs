using CSharpFunctionalExtensions;
using DirectoryService.Application.DataBase;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Positions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace DirectoryService.Application.DirectoryServiceManagement.Positions.Delete;

public class DeletePositionHandler
{
    private readonly IPositionsRepository _positionsRepository;
    private readonly ILogger<DeletePositionHandler> _logger;
    private readonly IValidator<DeletePositionCommand> _validator;
    private readonly ITransactionManager _transactionManager;

    public DeletePositionHandler(IPositionsRepository positionsRepository,
        ILogger<DeletePositionHandler> logger,
        IValidator<DeletePositionCommand> validator,
        ITransactionManager transactionManager)
    {
        _positionsRepository = positionsRepository;
        _logger = logger;
        _validator = validator;
        _transactionManager = transactionManager;
    }

    public async Task<Result<Guid, Errors>> Handle(
        DeletePositionCommand deletePositionCommand, 
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(deletePositionCommand, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }
        
        var positionId = new PositionId(deletePositionCommand.PositionId);

        var positionResult = await _positionsRepository.GetById(positionId, cancellationToken);
        if (positionResult.IsFailure)
        {
            return positionResult.Error.ToErrors();
        }

        _positionsRepository.Delete(positionResult.Value);

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            return saveResult.Error.ToErrors();
        }
        
        _logger.LogInformation("Position deleted with id={Id}", positionId.Value);

        return positionId.Value;
    }
}