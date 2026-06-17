using CSharpFunctionalExtensions;
using DirectoryService.Application.DataBase;
using DirectoryService.Application.DirectoryServiceManagement.Departments.LinkDepartmentAndLocation;
using DirectoryService.Application.DirectoryServiceManagement.Locations;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace DirectoryService.Application.DirectoryServiceManagement.Departments.UnLinkDepartmentAndLocationHandler;

public class UnLinkDepartmentAndLocationHandler(
    ILocationsRepository locationsRepository,
    IDepartmentsRepository departmentsRepository,
    ILogger<LinkDepartmentAndLocationHandler> logger,
    IValidator<DepartmentAndLocationCommand> validator,
    ITransactionManager transactionManager)
{
    private readonly ITransactionManager _transactionManager = transactionManager;

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

        if (!linkDepartmentAndLocationResult.Value)
        {
            return GeneralErrors.AlreadyExist("Department and Location not linked").ToErrors();
        }

        await departmentsRepository.DeleteLocationsByDepartmentId(departmentId, cancellationToken);
        
        logger.LogInformation("Location with id {locationId} and department with id {departmentId} unlinked", locationId.Value, departmentId.Value);
        
        return locationId.Value;
    }
}