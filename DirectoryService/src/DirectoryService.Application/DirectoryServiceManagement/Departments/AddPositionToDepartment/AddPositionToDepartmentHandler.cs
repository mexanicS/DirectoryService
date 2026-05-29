using CSharpFunctionalExtensions;
using DirectoryService.Application.DataBase;
using DirectoryService.Application.DirectoryServiceManagement.Departments.Delete;
using DirectoryService.Application.DirectoryServiceManagement.Positions;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace DirectoryService.Application.DirectoryServiceManagement.Departments.AddPositionToDepartment;

public class AddPositionToDepartmentHandler
{
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly IPositionsRepository _positionsRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly IValidator<AddPositionToDepartmentCommand> _validator;
    private readonly ILogger<DeleteDepartmentHandler> _logger;

    public AddPositionToDepartmentHandler(
        IDepartmentsRepository departmentsRepository,
        IPositionsRepository positionsRepository,
        ITransactionManager transactionManager,
        IValidator<AddPositionToDepartmentCommand> validator,
        ILogger<DeleteDepartmentHandler> logger)
    {
        _departmentsRepository = departmentsRepository;
        _positionsRepository = positionsRepository;
        _transactionManager = transactionManager;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(AddPositionToDepartmentCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        var positionId = new PositionId(command.PositionId);
        
        var positionResult = await _positionsRepository.GetById(positionId, cancellationToken);
        if (positionResult.IsFailure)
        {
            return positionResult.Error.ToErrors();
        }

        var departmentId = new DepartmentId(command.DepartmentId);
        
        var departmentResult = await _departmentsRepository.GetByIdWithPositions(departmentId, cancellationToken);
        if (departmentResult.IsFailure)
        {
            return departmentResult.Error.ToErrors();
        }

        var addResult = departmentResult.Value.AddPosition(command.PositionId);
        if (addResult.IsFailure)
        {
            return addResult.Error.ToErrors();
        }

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            return saveResult.Error.ToErrors();
        }
        
        _logger.LogInformation("Position with id={positionId} added to Department with id={departmentId}",
            command.PositionId, command.DepartmentId);

        return departmentResult.Value.Id.Value;
    }
}