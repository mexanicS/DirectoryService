using CSharpFunctionalExtensions;
using DirectoryService.Application.DataBase;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace DirectoryService.Application.DirectoryServiceManagement.Departments.Delete;

public class DeleteDepartmentHandler
{
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILogger<DeleteDepartmentHandler> _logger;
    private readonly IValidator<DeleteDepartmentCommand> _validator;
    private readonly ITransactionManager _transactionManager;

    public DeleteDepartmentHandler(IDepartmentsRepository departmentsRepository,
        ILogger<DeleteDepartmentHandler> logger,
        IValidator<DeleteDepartmentCommand> validator,
        ITransactionManager transactionManager)
    {
        _departmentsRepository = departmentsRepository;
        _logger = logger;
        _validator = validator;
        _transactionManager = transactionManager;
    }

    
    public async Task<Result<Guid, Errors>> Handle(
            DeleteDepartmentCommand deleteDepartmentCommand,
            CancellationToken cancellationToken)
        {
            ValidationResult? validationResult =
                await _validator.ValidateAsync(deleteDepartmentCommand, cancellationToken);
            if (!validationResult.IsValid)
            {
                return validationResult.ToErrors();
            }

            DepartmentId departmentId = new(deleteDepartmentCommand.DepartmentId);

            // 1. Открываем транзакцию
            var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
            if (transactionScopeResult.IsFailure)
            {
                return transactionScopeResult.Error.ToErrors();
            }

            var transactionScope = transactionScopeResult.Value;

            try
            {
                // 2. Получаем отдел и список его детей (рекурсивно загруженных через навигацию)
                var dataResult =
                    await _departmentsRepository.GetDepartmentWithChildren(departmentId, cancellationToken);
                if (dataResult.IsFailure)
                {
                    transactionScope.Rollback();
                    return dataResult.Error.ToErrors();
                }

                var (targetDepartment, children) = dataResult.Value;

                // 3. Делегируем расчет путей домену
                var oldPathPrefix = targetDepartment.Path.Value;
                var newPathPrefix = targetDepartment.GetParentPathValue();

                // 4. Пересчитываем иерархию для всех дочерних элементов
                foreach (var child in children)
                {
                    // Если это прямой потомок — перепривязываем к "дедушке"
                    if (child.ParentId == targetDepartment.Id)
                    {
                        child.UpdateParent(targetDepartment.ParentId);
                    }

                    // Сдвигаем узел вверх по дереву внутри домена
                    child.MoveUpInHierarchy(oldPathPrefix, newPathPrefix, targetDepartment.ParentId);
                }

                // 5. Удаляем сам выбранный отдел
                _departmentsRepository.Delete(targetDepartment);

                // 6. Сохраняем изменения (EF Core сгенерирует UPDATE для детей и DELETE для родителя)
                var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
                if (saveResult.IsFailure)
                {
                    transactionScope.Rollback();
                    return saveResult.Error.ToErrors();
                }

                // 7. Коммитим транзакцию
                var commitResult = transactionScope.Commit();
                if (commitResult.IsFailure)
                {
                    transactionScope.Rollback();
                    return commitResult.Error.ToErrors();
                }

                _logger.LogInformation("Department id={Id} deleted. Children shifted up successfully",
                    deleteDepartmentCommand.DepartmentId);

                return departmentId.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during shifting up hierarchy for department id={Id}",
                    deleteDepartmentCommand.DepartmentId);
                transactionScope.Rollback();
                throw;
            }
        }
}
