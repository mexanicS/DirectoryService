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
    private readonly ILogger<CreateDepartmentHandler> _logger;
    private readonly IValidator<UpdateDepartmentDto> _validator;
    private readonly ITransactionManager _transactionManager;

    public UpdateDepartmentHandler(IDepartmentsRepository departmentsRepository,
        ILocationsRepository locationsRepository,
        ILogger<CreateDepartmentHandler> logger,
        IValidator<UpdateDepartmentDto> validator,
        ITransactionManager transactionManager)
    {
        _departmentsRepository = departmentsRepository;
        _locationsRepository = locationsRepository;
        _logger = logger;
        _validator = validator;
        _transactionManager = transactionManager;
    }

    public async Task<Result<Guid, Errors>> Handle(
        UpdateDepartmentDto updateDepartmentDto,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(updateDepartmentDto, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        var department = await _departmentsRepository.GetById(updateDepartmentDto.DepartmentId, cancellationToken);
        if (department.IsFailure)
        {
            return department.Error.ToErrors();
        }

        var name = DepartmentName.Create(updateDepartmentDto.Name);
        if (name.IsFailure)
        {
            return name.Error.ToErrors();
        }

        var identifier = Identifier.Create(updateDepartmentDto.Identifier);
        if (identifier.IsFailure)
        {
            return identifier.Error.ToErrors();
        }

        var updateMainInformationResult = department.Value.UpdateMainInformation(name.Value, identifier.Value);

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);

        if (saveResult.IsFailure)
        {
            return saveResult.Error.ToErrors();
        }

        _logger.LogInformation("Department with id={Id}, has updated", updateDepartmentDto.DepartmentId);

        return department.Value.Id.Value;
    }
}