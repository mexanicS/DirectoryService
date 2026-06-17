using CSharpFunctionalExtensions;
using DirectoryService.Application.DataBase;
using DirectoryService.Application.DirectoryServiceManagement.Locations;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace DirectoryService.Application.DirectoryServiceManagement.Departments.LinkDepartmentAndLocation;

public class LinkDepartmentAndLocationHandler(
    ILocationsRepository locationsRepository,
    IDepartmentsRepository departmentsRepository,
    ILogger<LinkDepartmentAndLocationHandler> logger,
    IValidator<DepartmentAndLocationCommand> validator,
    ITransactionManager transactionManager)
{
    public async Task<Result<Guid, Errors>> Handle(DepartmentAndLocationCommand departmentAndLocationCommand, 
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(departmentAndLocationCommand, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }
        var locationId = new LocationId(departmentAndLocationCommand.LocationId);
        var departmentId = new DepartmentId(departmentAndLocationCommand.DepartmentId);
        
        var locationExistsResult = await locationsRepository.ExistsActiveLocationById(locationId, cancellationToken);
        if (locationExistsResult.IsFailure)
        {
            return locationExistsResult.Error.ToErrors();
        }
        
        var departmentExistsResult = await departmentsRepository.ExistsActiveDepartmentById(departmentId, cancellationToken);
        if (departmentExistsResult.IsFailure)
        {
            return departmentExistsResult.Error.ToErrors();
        }
        
        var linkDepartmentAndLocationResult = await departmentsRepository.ExistsLinkDepartmentAndLocation(departmentId, locationId, cancellationToken);

        if (linkDepartmentAndLocationResult.Value)
        {
            return GeneralErrors.AlreadyExist("Department and Location already linked").ToErrors();
        }

        var departmentLocations = DepartmentLocation.Create(departmentId, locationId);
        
        await departmentsRepository.AddDepartmentLocations(departmentLocations.Value, cancellationToken);
        
        var saveResult = await transactionManager.SaveChangesAsync(cancellationToken);

        if (saveResult.IsFailure)
        {
            return saveResult.Error.ToErrors();
        }
        
        logger.LogInformation("Location with id {locationId} and department with id {departmentId} linked", locationId.Value, departmentId.Value);
        
        
        return locationId.Value;
    }
}