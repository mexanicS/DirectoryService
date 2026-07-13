using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Application.DirectoryServiceManagement.Locations;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.DepartmentLocations;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace DirectoryService.Application.DirectoryServiceManagement.Departments.UpdateLocations;

public class UpdateLocationsByDepartmentHandler(
    IDepartmentsRepository departmentsRepository,
    ILocationsRepository locationsRepository,
    ILogger<UpdateLocationsByDepartmentHandler> logger,
    IValidator<UpdateLocationsByDepartmentCommand> validator,
    ITransactionManager transactionManager)
{
    public async Task<Result<Guid, Errors>> Handle(
        UpdateLocationsByDepartmentCommand updateDepartmentCommand,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(updateDepartmentCommand, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }
        
        var transactionScopeResult = await transactionManager.BeginTransactionAsync(cancellationToken);
        
        if (transactionScopeResult.IsFailure)
            return transactionScopeResult.Error.ToErrors();

        var transactionScope = transactionScopeResult.Value;
        
        var department = await departmentsRepository.GetById(updateDepartmentCommand.DepartmentId, cancellationToken);
        if (department.IsFailure)
        {
            return department.Error.ToErrors();
        }
        
        var locationExists = await locationsRepository.GetActiveLocationsById(updateDepartmentCommand.LocationIds, cancellationToken);
        if (locationExists.IsFailure)
        {
            return locationExists.Error.ToErrors();
        }
        
        var departmentLocations = locationExists.Value
            .Select(location => DepartmentLocation.Create(department.Value.Id, location.Id))
            .ToList();
        
        await departmentsRepository.DeleteLocationsByDepartmentId(department.Value.Id, cancellationToken);

        await departmentsRepository.AddDepartmentLocations(departmentLocations.Select(result => result.Value), cancellationToken);
        
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
        
        logger.LogInformation("Department with id={Id}, has updated locations", updateDepartmentCommand.DepartmentId);
        
        return department.Value.Id.Value;
    }
    
}