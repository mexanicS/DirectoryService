using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Application.DirectoryServiceManagement.Departments.Delete;
using DirectoryService.Application.DirectoryServiceManagement.Positions;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace DirectoryService.Application.DirectoryServiceManagement.Departments.AddPositionToDepartment;

public class AddPositionToDepartmentHandler(
    IDepartmentsRepository departmentsRepository,
    IPositionsRepository positionsRepository,
    ITransactionManager transactionManager,
    IValidator<AddPositionToDepartmentCommand> validator,
    ILogger<DeleteDepartmentHandler> logger)
{
    public async Task<Result<Guid, Errors>> Handle(AddPositionToDepartmentCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        var positionId = new PositionId(command.PositionId);
        
        var positionResult = await positionsRepository.GetById(positionId, cancellationToken);
        if (positionResult.IsFailure)
        {
            return positionResult.Error.ToErrors();
        }

        var departmentId = new DepartmentId(command.DepartmentId);
        
        var departmentResult = await departmentsRepository.GetByIdWithPositions(departmentId, cancellationToken);
        if (departmentResult.IsFailure)
        {
            return departmentResult.Error.ToErrors();
        }

        var addResult = departmentResult.Value.AddPosition(command.PositionId);
        if (addResult.IsFailure)
        {
            return addResult.Error.ToErrors();
        }

        var saveResult = await transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            return saveResult.Error.ToErrors();
        }
        
        logger.LogInformation("Position with id={positionId} added to Department with id={departmentId}",
            command.PositionId, command.DepartmentId);

        return departmentResult.Value.Id.Value;
    }
}