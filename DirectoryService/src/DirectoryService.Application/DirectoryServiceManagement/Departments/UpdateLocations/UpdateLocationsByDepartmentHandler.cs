using CSharpFunctionalExtensions;
using DirectoryService.Application.DataBase;
using DirectoryService.Application.DirectoryServiceManagement.DTOs;
using DirectoryService.Application.DirectoryServiceManagement.Locations;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.DepartmentLocations;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace DirectoryService.Application.DirectoryServiceManagement.Departments.UpdateLocations;

public class UpdateLocationsByDepartmentHandler
{
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILocationsRepository _locationsRepository;
    private readonly ILogger<UpdateLocationsByDepartmentHandler> _logger;
    private readonly IValidator<UpdateLocationsByDepartmentDto> _validator;
    private readonly ITransactionManager _transactionManager;

    public UpdateLocationsByDepartmentHandler(IDepartmentsRepository departmentsRepository,
        ILocationsRepository locationsRepository,
        ILogger<UpdateLocationsByDepartmentHandler> logger,
        IValidator<UpdateLocationsByDepartmentDto> validator, 
        ITransactionManager transactionManager)
    {
        _departmentsRepository = departmentsRepository;
        _locationsRepository = locationsRepository;
        _logger = logger;
        _validator = validator;
        _transactionManager = transactionManager;
    }

    public async Task<Result<Guid, Errors>> Handle(
        UpdateLocationsByDepartmentDto updateDepartmentDto,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(updateDepartmentDto, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }
        
        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        
        if (transactionScopeResult.IsFailure)
            return transactionScopeResult.Error.ToErrors();

        var transactionScope = transactionScopeResult.Value;
        
        var department = await _departmentsRepository.GetById(updateDepartmentDto.DepartmentId, cancellationToken);
        if (department.IsFailure)
        {
            return department.Error.ToErrors();
        }
        
        var locationExists = await _locationsRepository.GetActiveLocationsById(updateDepartmentDto.LocationIds, cancellationToken);
        if (locationExists.IsFailure)
        {
            return locationExists.Error.ToErrors();
        }
        
        var departmentLocations = locationExists.Value
            .Select(location => DepartmentLocation.Create(department.Value.Id, location.Id))
            .ToList();
        
        await _departmentsRepository.DeleteLocationsByDepartmentId(department.Value.Id, cancellationToken);

        await _departmentsRepository.AddDepartmentLocations(departmentLocations.Select(result => result.Value), cancellationToken);
        
        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);

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
        
        _logger.LogInformation("Department with id={Id}, has updated locations", updateDepartmentDto.DepartmentId);
        
        return department.Value.Id.Value;
    }
    
}