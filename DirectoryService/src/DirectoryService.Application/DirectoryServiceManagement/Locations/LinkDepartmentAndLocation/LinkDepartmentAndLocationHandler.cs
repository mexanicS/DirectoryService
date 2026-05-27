using CSharpFunctionalExtensions;
using DirectoryService.Application.DataBase;
using DirectoryService.Application.DirectoryServiceManagement.Departments;
using DirectoryService.Application.DirectoryServiceManagement.DTOs;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace DirectoryService.Application.DirectoryServiceManagement.Locations.LinkDepartmentAndLocation;

public class LinkDepartmentAndLocationHandler
{
    private readonly ILocationsRepository _locationsRepository;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILogger<LinkDepartmentAndLocationHandler> _logger;
    private readonly IValidator<LinkDepartmentAndLocationCommand> _validator;
    private readonly ITransactionManager _transactionManager;

    public LinkDepartmentAndLocationHandler(ILocationsRepository locationsRepository,
        IDepartmentsRepository departmentsRepository,
        ILogger<LinkDepartmentAndLocationHandler> logger,
        IValidator<LinkDepartmentAndLocationCommand> validator,
        ITransactionManager  transactionManager
        )
    {
        _locationsRepository = locationsRepository;
        _departmentsRepository = departmentsRepository;
        _logger = logger;
        _validator = validator;
        _transactionManager = transactionManager;
    }

    public async Task<Result<Guid, Errors>> Handle(LinkDepartmentAndLocationCommand linkDepartmentAndLocationCommand, 
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(linkDepartmentAndLocationCommand, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }
        var locationId = new LocationId(linkDepartmentAndLocationCommand.LocationId);
        var departmentId = new DepartmentId(linkDepartmentAndLocationCommand.DepartmentId);
        
        var locationExistsResult = await _locationsRepository.ExistsActiveLocationById(locationId, cancellationToken);
        if (locationExistsResult.IsFailure)
        {
            return locationExistsResult.Error.ToErrors();
        }
        
        var departmentExistsResult = await _departmentsRepository.ExistsActiveDepartmentById(departmentId, cancellationToken);
        if (departmentExistsResult.IsFailure)
        {
            return departmentExistsResult.Error.ToErrors();
        }
        
        var linkDepartmentAndLocationResult = await _departmentsRepository.LinkDepartmentAndLocation(departmentId, locationId, cancellationToken);

        if (linkDepartmentAndLocationResult.IsFailure)
        {
            return GeneralErrors.AlreadyExist("Department and Location already linked").ToErrors();
        }

        var departmentLocations = DepartmentLocation.Create(departmentId, locationId);
        
        await _departmentsRepository.AddDepartmentLocations(departmentLocations.Value, cancellationToken);
        
        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);

        if (saveResult.IsFailure)
        {
            return saveResult.Error.ToErrors();
        }
        
        _logger.LogInformation("Location with id {locationId} and department with id {departmentId} linked", locationId.Value, departmentId.Value);
        
        
        return locationId.Value;
    }
}