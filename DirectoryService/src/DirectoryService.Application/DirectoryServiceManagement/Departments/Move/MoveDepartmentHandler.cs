using System.Data;
using System.Data.Common;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Contract;
using DirectoryService.Domain.Departments;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace DirectoryService.Application.DirectoryServiceManagement.Departments.Move;

public sealed class MoveDepartmentHandler(
    IDepartmentsRepository departmentsRepository,
    ITransactionManager transactionManager,
    IValidator<MoveDepartmentCommand> validator,
    ILogger<MoveDepartmentHandler> logger)
{
    public async Task<Result<MoveDepartmentResponse, Errors>> Handle(
        MoveDepartmentCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        var departmentId = new DepartmentId(command.DepartmentId);

        var transactionResult = await transactionManager.BeginTransactionAsync(
            cancellationToken);
        if (transactionResult.IsFailure)
        {
            return transactionResult.Error.ToErrors();
        }

        using var transaction = transactionResult.Value;

        try
        {
            var lockIds = command.ParentId is { } parentId
                ? new[] { departmentId, new DepartmentId(parentId) }
                : [departmentId];

            var lockedDepartments = await departmentsRepository.LockMoveSnapshots(lockIds, cancellationToken);
            var department = lockedDepartments.SingleOrDefault(d => d.Id == departmentId);

            if (command.ParentId == command.DepartmentId)
            {
                transaction.Rollback();
                return MoveDepartmentErrors.ParentIsSelf().ToErrors();
            }

            if (department is null || department.IsDeleted)
            {
                transaction.Rollback();
                return MoveDepartmentErrors.DepartmentNotFound(command.DepartmentId).ToErrors();
            }

            DepartmentMoveSnapshot? parent = null;
            if (command.ParentId is { } parentGuid)
            {
                parent = lockedDepartments.SingleOrDefault(d => d.Id.Value == parentGuid);
                if (parent is null)
                {
                    transaction.Rollback();
                    return MoveDepartmentErrors.ParentNotFound(parentGuid).ToErrors();
                }

                if (parent.IsDeleted)
                {
                    transaction.Rollback();
                    return MoveDepartmentErrors.ParentDeleted().ToErrors();
                }

                if (IsSameOrDescendant(parent.Path, department.Path))
                {
                    transaction.Rollback();
                    return MoveDepartmentErrors.Cycle().ToErrors();
                }
            }

            var currentParentId = department.ParentId?.Value;
            if (currentParentId == command.ParentId)
            {
                transaction.Rollback();
                return ToResponse(department);
            }

            var pathConflictExists = await departmentsRepository.ExistsActiveSiblingWithIdentifier(
                parent?.Id,
                department.Id,
                department.Identifier,
                cancellationToken);
            if (pathConflictExists)
            {
                transaction.Rollback();
                return MoveDepartmentErrors.PathConflict().ToErrors();
            }

            var newPath = parent is null
                ? department.Identifier
                : $"{parent.Path}.{department.Identifier}";
            
            var newDepth = parent is null ? 0 : parent.Depth + 1;
            var updatedAt = DateTime.UtcNow;

            var updateResult = await departmentsRepository.MoveSubtree(
                department.Id,
                parent?.Id,
                department.Path,
                newPath,
                newDepth - department.Depth,
                updatedAt,
                cancellationToken);

            if (updateResult.IsFailure)
            {
                transaction.Rollback();
                return updateResult.Error.ToErrors();
            }

            if (updateResult.Value == 0)
            {
                transaction.Rollback();
                return MoveDepartmentErrors.DepartmentNotFound(command.DepartmentId).ToErrors();
            }

            var commitResult = transaction.Commit();
            if (commitResult.IsFailure)
            {
                transaction.Rollback();
                return commitResult.Error.Type == ErrorType.CONFLICT
                    ? MoveDepartmentErrors.TransactionConflict().ToErrors()
                    : commitResult.Error.ToErrors();
            }

            logger.LogInformation("Department id={DepartmentId} moved to parent id={ParentId}",
                command.DepartmentId, command.ParentId);

            return new MoveDepartmentResponse(
                command.DepartmentId,
                command.ParentId,
                newPath,
                newDepth,
                updatedAt);
        }
        catch (Exception ex) when (IsTransactionConflict(ex))
        {
            logger.LogWarning(ex,
                "Concurrent department move conflict for department id={DepartmentId}",
                command.DepartmentId);
            transaction.Rollback();
            return MoveDepartmentErrors.TransactionConflict().ToErrors();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to move department id={DepartmentId}", command.DepartmentId);
            transaction.Rollback();
            return Error.Failure("department.move.failed", "Failed to move department.").ToErrors();
        }
    }

    private static bool IsSameOrDescendant(string candidatePath, string subtreePath) =>
        candidatePath == subtreePath || candidatePath.StartsWith(subtreePath + '.', StringComparison.Ordinal);

    private static bool IsTransactionConflict(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is DbException { SqlState: { } sqlState } &&
                (sqlState.StartsWith("40", StringComparison.Ordinal) || sqlState == "55P03"))
            {
                return true;
            }
        }

        return false;
    }

    private static MoveDepartmentResponse ToResponse(DepartmentMoveSnapshot department) =>
        new(
            department.Id.Value,
            department.ParentId?.Value,
            department.Path,
            department.Depth,
            department.UpdatedAt);
}
