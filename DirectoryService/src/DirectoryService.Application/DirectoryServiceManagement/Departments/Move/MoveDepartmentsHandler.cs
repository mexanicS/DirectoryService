using CSharpFunctionalExtensions;
using DirectoryService.Application.DataBase;
using DirectoryService.Application.DirectoryServiceManagement.Departments.Create;
using DirectoryService.Application.DirectoryServiceManagement.DTOs;
using DirectoryService.Application.DirectoryServiceManagement.Locations;
using DirectoryService.Application.Validation;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace DirectoryService.Application.DirectoryServiceManagement.Departments.Move;

public class MoveDepartmentsHandler 
{
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILocationsRepository _locationsRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<MoveDepartmentsHandler> _logger;
    private readonly IValidator<MoveDepartmentsDto> _validator;

    public MoveDepartmentsHandler(IDepartmentsRepository departmentsRepository,
        ILocationsRepository locationsRepository,
        ITransactionManager transactionManager,
        ILogger<MoveDepartmentsHandler> logger,
        IValidator<MoveDepartmentsDto> validator)
    {
        _departmentsRepository = departmentsRepository;
        _locationsRepository = locationsRepository;
        _transactionManager = transactionManager;
        _logger = logger;
        _validator = validator;
    }
     public async Task<Result<Guid, Errors>> Handle(
        MoveDepartmentsDto moveDepartmentsDto,
        CancellationToken cancellationToken)
    {   
        var validationResult = await _validator.ValidateAsync(moveDepartmentsDto, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }
    }
}