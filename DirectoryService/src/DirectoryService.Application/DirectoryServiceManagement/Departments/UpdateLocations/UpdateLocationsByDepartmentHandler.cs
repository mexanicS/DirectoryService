using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using DirectoryService.Application.DirectoryServiceManagement.DTOs;
using DirectoryService.Application.DirectoryServiceManagement.Locations;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
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

    public UpdateLocationsByDepartmentHandler(IDepartmentsRepository departmentsRepository,
        ILocationsRepository locationsRepository,
        ILogger<UpdateLocationsByDepartmentHandler> logger,
        IValidator<UpdateLocationsByDepartmentDto> validator)
    {
        _departmentsRepository = departmentsRepository;
        _locationsRepository = locationsRepository;
        _logger = logger;
        _validator = validator;
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
        
        var departmentExists = await _departmentsRepository.DepartmentExists([updateDepartmentDto.DepartmentId], cancellationToken);
        if (departmentExists.IsFailure)
        {
            return departmentExists.Error.ToErrors();
        }
        
        var locationExists = await _locationsRepository.ExistsActiveLocationsById(updateDepartmentDto.LocationIds, cancellationToken);
        if (locationExists.IsFailure)
        {
            return locationExists.Error.ToErrors();
        }
        
        var departmentId = new DepartmentId(updateDepartmentDto.DepartmentId);
        var departmentResult = await _departmentsRepository.GetByIdWithLocations(departmentId,
            cancellationToken);
        
        if (departmentResult.IsFailure)
            return departmentResult.Error.ToErrors();

        departmentResult.Value.UpdateLocations(updateDepartmentDto.LocationIds);

        return departmentResult.Value.Id.Value;
    }
    
}