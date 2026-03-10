using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using DirectoryService.Application.DirectoryServiceManagement.DTOs;
using DirectoryService.Application.DirectoryServiceManagement.Locations;
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
        UpdateLocationsByDepartmentDto createDepartmentDto,
        CancellationToken cancellationToken)
    {
        
    }
    
}