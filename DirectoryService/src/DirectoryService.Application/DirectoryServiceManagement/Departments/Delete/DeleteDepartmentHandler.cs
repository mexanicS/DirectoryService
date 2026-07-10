using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace DirectoryService.Application.DirectoryServiceManagement.Departments.Delete;

public class DeleteDepartmentHandler(
    IDepartmentsRepository departmentsRepository,
    ILogger<DeleteDepartmentHandler> logger,
    IValidator<DeleteDepartmentCommand> validator,
    ITransactionManager transactionManager)
{
    public async Task<Result<Guid, Errors>> Handle(
            DeleteDepartmentCommand deleteDepartmentCommand,
            CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(deleteDepartmentCommand, cancellationToken);
            if (!validationResult.IsValid)
            {
                return validationResult.ToErrors();
            }

            DepartmentId departmentId = new(deleteDepartmentCommand.DepartmentId);

            var transactionScopeResult = await transactionManager.BeginTransactionAsync(cancellationToken);
            if (transactionScopeResult.IsFailure)
            {
                return transactionScopeResult.Error.ToErrors();
            }

            var transactionScope = transactionScopeResult.Value;

            try
            {
                var dataResult =
                    await departmentsRepository.GetDepartmentWithChildren(departmentId, cancellationToken);
                if (dataResult.IsFailure)
                {
                    transactionScope.Rollback();
                    return dataResult.Error.ToErrors();
                }

                var (targetDepartment, children) = dataResult.Value;

                var oldPathPrefix = targetDepartment.Path.Value;
                var newPathPrefix = targetDepartment.GetParentPathValue();

                foreach (var child in children)
                {
                    if (child.ParentId == targetDepartment.Id)
                    {
                        child.UpdateParent(targetDepartment.ParentId);
                    }

                    child.MoveUpInHierarchy(oldPathPrefix, newPathPrefix, targetDepartment.ParentId);
                }

                departmentsRepository.Delete(targetDepartment);

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

                logger.LogInformation("Department id={Id} deleted. Children shifted up successfully",
                    deleteDepartmentCommand.DepartmentId);

                return departmentId.Value;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during shifting up hierarchy for department id={Id}",
                    deleteDepartmentCommand.DepartmentId);
                
                transactionScope.Rollback();
                
                return GeneralErrors.Failure(ex.Message).ToErrors();
            }
        }
}
