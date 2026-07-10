using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Application.DirectoryServiceManagement.Departments.Delete;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace DirectoryService.Application.DirectoryServiceManagement.Departments.RemovePositionFromDepartment;

public class RemovePositionFromDepartmentHandler(
    IDepartmentsRepository departmentsRepository,
    ITransactionManager transactionManager,
    IValidator<RemovePositionFromDepartmentCommand> validator,
    ILogger<DeleteDepartmentHandler> logger)
{
    public async Task<Result<Guid, Errors>> Handle(RemovePositionFromDepartmentCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        var departmentId = new DepartmentId(command.DepartmentId);

        var departmentResult = await departmentsRepository.GetByIdWithPositions(departmentId, cancellationToken);
        if (departmentResult.IsFailure)
        {
            return departmentResult.Error.ToErrors();
        }

        var removeResult = departmentResult.Value.RemovePosition(command.PositionId);
        if (removeResult.IsFailure)
        {
            return removeResult.Error.ToErrors();
        }

        var saveResult = await transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            return saveResult.Error.ToErrors();
        }
        
        logger.LogInformation("Position with id={positionId} remove in Department with id={departmentId}",
            command.PositionId, command.DepartmentId);
        
        return departmentResult.Value.Id.Value;
    }
}