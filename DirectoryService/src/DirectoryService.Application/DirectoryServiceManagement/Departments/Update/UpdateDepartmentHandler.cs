using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Application.DirectoryServiceManagement.Locations;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace DirectoryService.Application.DirectoryServiceManagement.Departments.Update;

public class UpdateDepartmentHandler(
    IDepartmentsRepository departmentsRepository,
    ILocationsRepository locationsRepository,
    ILogger<UpdateDepartmentHandler> logger,
    IValidator<UpdateDepartmentCommand> validator,
    ITransactionManager transactionManager)
{
    private readonly ILocationsRepository _locationsRepository = locationsRepository;

    public async Task<Result<Guid, Errors>> Handle(
        UpdateDepartmentCommand updateDepartmentCommand,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(updateDepartmentCommand, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        var department = await departmentsRepository.GetById(updateDepartmentCommand.DepartmentId, cancellationToken);
        if (department.IsFailure)
        {
            return department.Error.ToErrors();
        }

        var name = DepartmentName.Create(updateDepartmentCommand.Name);
        if (name.IsFailure)
        {
            return name.Error.ToErrors();
        }

        var identifier = Identifier.Create(updateDepartmentCommand.Identifier);
        if (identifier.IsFailure)
        {
            return identifier.Error.ToErrors();
        }

        department.Value.UpdateMainInformation(name.Value, identifier.Value);

        var saveResult = await transactionManager.SaveChangesAsync(cancellationToken);

        if (saveResult.IsFailure)
        {
            return saveResult.Error.ToErrors();
        }

        logger.LogInformation("Department with id={Id}, has updated", updateDepartmentCommand.DepartmentId);

        return department.Value.Id.Value;
    }
}