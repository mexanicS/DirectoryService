using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using DirectoryService.Application.DirectoryServiceManagement.Departments;
using DirectoryService.Application.DirectoryServiceManagement.DTOs;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace DirectoryService.Application.DirectoryServiceManagement.Positions.Create;

public class CreatePositionHandler
{
    private readonly IPositionsRepository _positionsRepository;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILogger<CreatePositionHandler> _logger;
    private readonly IValidator<CreatePositionDto> _validator;

    public CreatePositionHandler(IPositionsRepository positionsRepository,
        IDepartmentsRepository departmentsRepository,
        ILogger<CreatePositionHandler> logger,
        IValidator<CreatePositionDto> validator)
    {
        _positionsRepository = positionsRepository;
        _departmentsRepository = departmentsRepository;
        _logger = logger;
        _validator = validator;
    }

    public async Task<Result<Guid, Errors>> Handle(
        CreatePositionDto createPositionDto, 
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(createPositionDto, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }
        var positionId = new PositionId(Guid.NewGuid());
        var positionName = PositionName.Create(createPositionDto.Name).Value;
        var description = Description.Create(createPositionDto.Description).Value;
        var departmentIds = createPositionDto.DepartmentIds.ToList();
        
        var activePositionByName = await _positionsRepository.IsActivePositionByName(positionName, cancellationToken);
        if (activePositionByName.Value)
        {
            return GeneralErrors.AlreadyExist().ToErrors();
        }
        
        var locationExists = await _departmentsRepository.DepartmentExists(departmentIds, cancellationToken);
        if (locationExists.IsFailure)
        {
            return locationExists.Error.ToErrors();
        }

        var newPosition = Position.Create(positionId, positionName, description);

        var addPositionResult = await _positionsRepository.Add(newPosition.Value, cancellationToken);
        if (addPositionResult.IsFailure)
        {
            return addPositionResult.Error;
        }

        foreach (var departmentId in createPositionDto.DepartmentIds)
        {
            //TODO: переделать на один запрос
            var departmentResult = await _departmentsRepository
                .GetByIdWithPositions(new DepartmentId(departmentId), cancellationToken);

            if (departmentResult.IsFailure)
            {
                return departmentResult.Error.ToErrors();
            }

            var addPositionToDepartmentResult = departmentResult.Value.AddPosition(positionId.Value);
            if (addPositionToDepartmentResult.IsFailure)
            {
                return addPositionToDepartmentResult.Error.ToErrors();
            }
        }

        var saveResult = await _departmentsRepository.SaveChanges(cancellationToken);

        if (saveResult.IsFailure)
        {
            return saveResult.Error;
        }

        return newPosition.Value.Id.Value;
    }
}