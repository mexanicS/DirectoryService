using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Application.DirectoryServiceManagement.Departments.Delete;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace DirectoryService.Application.DirectoryServiceManagement.Departments.SoftDeleteDepartment;

public class SoftDeleteDepartmentHandler(
    IDepartmentsRepository departmentsRepository,
    ILogger<SoftDeleteDepartmentHandler> logger,
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
            
            var dataResult =
                await departmentsRepository.GetDepartmentWithChildren(departmentId, cancellationToken);
            if (dataResult.IsFailure)
            {
                return dataResult.Error.ToErrors();
            }

            var (targetDepartment, children) = dataResult.Value;

            children.Add(targetDepartment);

            foreach (var department in children)
            {
                department.SoftDelete();
            }
            
            var saveResult = await transactionManager.SaveChangesAsync(cancellationToken);
            if (saveResult.IsFailure)
            {
                return saveResult.Error.ToErrors();
            }

            logger.LogInformation("Departments id={Id} are marked as deleted.",
                string.Join(",", children.Select(c => c.Id.Value)));

            return departmentId.Value;
        }
}
