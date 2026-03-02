using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using DirectoryService.Application.DirectoryServiceManagement.DTOs;
using DirectoryService.Application.DirectoryServiceManagement.Locations;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace DirectoryService.Application.DirectoryServiceManagement.Departments.Create;

public class CreateDepartmentHandler
{
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILocationsRepository _locationsRepository;
    private readonly ILogger<CreateDepartmentHandler> _logger;
    private readonly IValidator<CreateDepartmentDto> _validator;

    public CreateDepartmentHandler(IDepartmentsRepository departmentsRepository,
        ILocationsRepository locationsRepository,
        ILogger<CreateDepartmentHandler> logger,
        IValidator<CreateDepartmentDto> validator)
    {
        _departmentsRepository = departmentsRepository;
        _locationsRepository = locationsRepository;
        _logger = logger;
        _validator = validator;
    }

    public async Task<Result<Guid, Errors>> Handle(
        CreateDepartmentDto createDepartmentDto,
        CancellationToken cancellationToken)
    {   
        var validationResult = await _validator.ValidateAsync(createDepartmentDto, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }
        
        var departmentId = new DepartmentId(Guid.NewGuid());
        var name = DepartmentName.Create(createDepartmentDto.Name).Value;
        var identifier = Identifier.Create(createDepartmentDto.Identifier).Value;
        
        var locationIds = createDepartmentDto.LocationIds.ToList();
        
        var locationExists = await _locationsRepository.ExistsActiveLocationsById(locationIds, cancellationToken);
        if (locationExists.IsFailure)
        {
            return locationExists.Error.ToErrors();
        }
        
        var departmentLocations =
            createDepartmentDto.LocationIds.Select(id => DepartmentLocation.Create(departmentId, LocationId.Create(id)).Value);
        
        if (createDepartmentDto.ParentId is null)
        {
            var departmentResult = Department.CreateParent(
                name,
                identifier,
                departmentLocations,
                departmentId);

            if (departmentResult.IsFailure)
                return departmentResult.Error.ToErrors();

            var saveResult = await _departmentsRepository.Add(departmentResult.Value, cancellationToken);

            if (saveResult.IsFailure)
                return saveResult.Error;

            _logger.LogInformation("Department created with id={Id}", departmentId.Value);

            return departmentResult.Value.Id.Value;
        }
        else
        {
            var parentQuery = await _departmentsRepository.GetById((Guid)createDepartmentDto.ParentId, cancellationToken);

            if (parentQuery.IsFailure)
                return parentQuery.Error.ToErrors();

            var departmentResult = Department.CreateChild(
                name,
                identifier,
                parentQuery.Value,
                departmentLocations,
                departmentId);

            if (departmentResult.IsFailure)
            {
                return departmentResult.Error.ToErrors();
            }

            var saveResult = await _departmentsRepository.Add(departmentResult.Value, cancellationToken);

            if (saveResult.IsFailure)
            {
                return saveResult.Error;
            }
            
            _logger.LogInformation("Child department created with id={Id}", departmentId.Value);

            return departmentResult.Value.Id.Value;
        }
    }
    
}