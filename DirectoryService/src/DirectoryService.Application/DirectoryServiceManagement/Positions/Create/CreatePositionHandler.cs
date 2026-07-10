using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Application.DirectoryServiceManagement.Departments;
using DirectoryService.Application.DirectoryServiceManagement.DTOs;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace DirectoryService.Application.DirectoryServiceManagement.Positions.Create;

public class CreatePositionHandler(
    IPositionsRepository positionsRepository,
    IDepartmentsRepository departmentsRepository,
    ILogger<CreatePositionHandler> logger,
    IValidator<CreatePositionDto> validator,
    ITransactionManager transactionManager)
{
    public async Task<Result<Guid, Errors>> Handle(
        CreatePositionDto createPositionDto, 
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(createPositionDto, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }
        
        var transactionScopeResult = await transactionManager.BeginTransactionAsync(cancellationToken);
        
        if (transactionScopeResult.IsFailure)
            return transactionScopeResult.Error.ToErrors();

        var transactionScope = transactionScopeResult.Value;
        
        var positionId = new PositionId(Guid.NewGuid());
        var positionName = PositionName.Create(createPositionDto.Name).Value;
        var description = Description.Create(createPositionDto.Description).Value;
        var departmentIds = createPositionDto.DepartmentIds.ToList();
        
        var activePositionByName = await positionsRepository.IsActivePositionByName(positionName, cancellationToken);
        if (activePositionByName.Value)
        {
            return GeneralErrors.AlreadyExist().ToErrors();
        }
        
        var locationExists = await departmentsRepository.DepartmentsExists(departmentIds, cancellationToken);
        if (locationExists.IsFailure)
        {
            return locationExists.Error.ToErrors();
        }

        var newPosition = Position.Create(positionId, positionName, description);

        var addPositionResult = await positionsRepository.Add(newPosition.Value, cancellationToken);
        if (addPositionResult.IsFailure)
        {
            return addPositionResult.Error;
        }
        
        var departmentsResult = await departmentsRepository
            .GetByIdsWithPositions(departmentIds, cancellationToken);

        if (departmentsResult.IsFailure)
        {
            return departmentsResult.Error.ToErrors();
        }
        
        var missingIds = departmentIds.Except(departmentsResult.Value.Select(d => d.Id.Value)).ToList();
        if (missingIds.Any())
        {
            return GeneralErrors.NotFound(missingIds, nameof(Department)).ToErrors();
        }
        
        foreach (var department in departmentsResult.Value)
        {
            var result = department.AddPosition(positionId.Value);
            if (result.IsFailure)
            {
                return result.Error.ToErrors();
            }
        }

        var saveResult = await transactionManager.SaveChangesAsync(cancellationToken);

        if (saveResult.IsFailure)
        {
            transactionScope.Rollback();

            return saveResult.Error.ToErrors();
        }
        
        var commitResult = transactionScope.Commit();

        if (commitResult.IsFailure)
        {
            transactionScope.Rollback();

            return commitResult.Error.ToErrors();
        }
        
        logger.LogInformation("Position created with id={Id}", positionId.Value);

        return newPosition.Value.Id.Value;
    }
}