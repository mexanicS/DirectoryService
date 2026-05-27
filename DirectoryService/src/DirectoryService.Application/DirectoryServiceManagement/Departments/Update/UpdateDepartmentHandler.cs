using CSharpFunctionalExtensions;
using DirectoryService.Application.DataBase;
using DirectoryService.Application.DirectoryServiceManagement.Departments.Create;
using DirectoryService.Application.DirectoryServiceManagement.DTOs;
using DirectoryService.Application.DirectoryServiceManagement.Locations;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace DirectoryService.Application.DirectoryServiceManagement.Departments.Update;

public class UpdateDepartmentHandler
{
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILocationsRepository _locationsRepository;
    private readonly ILogger<UpdateDepartmentHandler> _logger;
    private readonly IValidator<UpdateDepartmentCommand> _validator;
    private readonly ITransactionManager _transactionManager;

    public UpdateDepartmentHandler(IDepartmentsRepository departmentsRepository,
        ILocationsRepository locationsRepository,
        ILogger<UpdateDepartmentHandler> logger,
        IValidator<UpdateDepartmentCommand> validator,
        ITransactionManager transactionManager)
    {
        _departmentsRepository = departmentsRepository;
        _locationsRepository = locationsRepository;
        _logger = logger;
        _validator = validator;
        _transactionManager = transactionManager;
    }

    public async Task<Result<Guid, Errors>> Handle(
        UpdateDepartmentCommand updateDepartmentCommand,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(updateDepartmentCommand, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        var department = await _departmentsRepository.GetById(updateDepartmentCommand.DepartmentId, cancellationToken);
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

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);

        if (saveResult.IsFailure)
        {
            return saveResult.Error.ToErrors();
        }

        _logger.LogInformation("Department with id={Id}, has updated", updateDepartmentCommand.DepartmentId);

        return department.Value.Id.Value;
    }
}