using System.Data.Common;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.DirectoryServiceManagement.Departments;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace DirectoryService.Infrastructure.Repositories;

public class DepartmentRepository(
    DirectoryServiceDbContext context,
    ILogger<DepartmentRepository> logger)
    : BaseRepository<Department>(context, logger), IDepartmentsRepository
{
    private readonly DirectoryServiceDbContext _context = context;

    public async Task<IReadOnlyList<DepartmentMoveSnapshot>> LockMoveSnapshots(
        IReadOnlyCollection<DepartmentId> departmentIds,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT d.id AS "Id",
                   d.parent_id AS "ParentId",
                   d.identifier AS "Identifier",
                   d.path::text AS "Path",
                   d.depth AS "Depth",
                   d.is_deleted AS "IsDeleted",
                   d.update_at AS "UpdatedAt"
            FROM "DirectoryService".department AS d
            WHERE d.id = ANY(@DepartmentIds)
            ORDER BY d.id ASC
            FOR UPDATE OF d
            """;

        var connection = _context.Database.GetDbConnection();
        var transaction = _context.Database.CurrentTransaction?.GetDbTransaction()
            ?? throw new InvalidOperationException("Department move locks require an active transaction.");
        var command = new CommandDefinition(
            sql,
            new { DepartmentIds = departmentIds.Select(id => id.Value).Distinct().ToArray() },
            transaction,
            cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync<DepartmentMoveSnapshotRow>(command);

        return rows
            .Select(row => new DepartmentMoveSnapshot(
                new DepartmentId(row.Id),
                row.ParentId is { } parentId ? new DepartmentId(parentId) : null,
                row.Identifier,
                row.Path,
                row.Depth,
                row.IsDeleted,
                row.UpdatedAt))
            .ToList();
    }

    public async Task<bool> ExistsActiveSiblingWithIdentifier(
        DepartmentId? parentId,
        DepartmentId excludedDepartmentId,
        string identifier,
        CancellationToken cancellationToken)
    {
        var departmentIdentifier = Identifier.Create(identifier).Value;

        return await _context.Departments
            .IgnoreQueryFilters()
            .AnyAsync(
                department =>
                    department.ParentId == parentId &&
                    department.Id != excludedDepartmentId &&
                    department.Identifier == departmentIdentifier &&
                    department.IsActive &&
                    !department.IsDeleted,
                cancellationToken);
    }

    public async Task<Result<int, Error>> MoveSubtree(
        DepartmentId departmentId,
        DepartmentId? newParentId,
        string oldPath,
        string newPath,
        int depthDelta,
        DateTime updatedAt,
        CancellationToken cancellationToken)
    {
        try
        {
            const string sql = """
                UPDATE "DirectoryService".department AS d
                SET path = CASE
                        WHEN d.id = @DepartmentId THEN CAST(@NewPath AS ltree)
                        ELSE CAST(@NewPath AS ltree) || subpath(d.path, nlevel(CAST(@OldPath AS ltree)))
                    END,
                    depth = d.depth + @DepthDelta,
                    parent_id = CASE
                        WHEN d.id = @DepartmentId THEN @NewParentId
                        ELSE d.parent_id
                    END,
                    update_at = @UpdatedAt
                WHERE d.path <@ CAST(@OldPath AS ltree)
                """;

            var parameters = new
            {
                DepartmentId = departmentId.Value,
                NewParentId = newParentId?.Value,
                OldPath = oldPath,
                NewPath = newPath,
                DepthDelta = depthDelta,
                UpdatedAt = updatedAt
            };

            var connection = _context.Database.GetDbConnection();
            var transaction = _context.Database.CurrentTransaction?.GetDbTransaction();
            var command = new CommandDefinition(
                sql,
                parameters,
                transaction,
                cancellationToken: cancellationToken);

            var affectedRows = await connection.ExecuteAsync(command);

            return affectedRows;
        }
        catch (Exception ex) when (IsTransactionConflict(ex))
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to move department subtree for department id={DepartmentId}",
                departmentId.Value);
            return Error.Failure("department.move.failed", "Failed to move department subtree.");
        }
    }

    public async Task<Result<Guid, Errors>> Add(Department department,
        CancellationToken cancellationToken)
    {
        try
        {
            await _context.Departments.AddAsync(department, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return department.Id.Value;
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            logger.LogWarning(ex, "Location with this name already exists");
            return GeneralErrors.AlreadyExist().ToErrors();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error to add department");
            return GeneralErrors.Failure().ToErrors();
        }
    }

    public async Task<Result<Department, Error>> GetById(Guid parentId, CancellationToken cancellationToken)
    {
        var department =
            await _context.Departments.FirstOrDefaultAsync(d => d.Id == parentId && d.IsActive, cancellationToken);

        if (department is null)
        {
            return GeneralErrors.NotFound(parentId, nameof(Department));
        }

        return department;
    }

    public async Task<Result<bool, Error>> DepartmentsExists(IEnumerable<Guid> departmentIds,
        CancellationToken cancellationToken)
    {
        var departmentIdsDistinct = departmentIds.Distinct().ToArray();
        var expectedCount = departmentIdsDistinct.Length;

        var actualCount = await _context.Departments
            .CountAsync(d => departmentIdsDistinct.Contains(d.Id) && d.IsActive, cancellationToken);

        return expectedCount == actualCount
            ? true
            : Error.NotFound("department.id", $"Found {actualCount}/{expectedCount} departments");
    }

    public async Task<Result<Department, Error>> GetByIdWithPositions(DepartmentId id,
        CancellationToken cancellationToken)
    {
        var department = await _context.Departments
            .Include(d => d.DepartmentPositions)
            .FirstOrDefaultAsync(d => d.Id == id && d.IsActive, cancellationToken);

        if (department is null)
        {
            return GeneralErrors.NotFound(id.Value, nameof(Department));
        }

        return department;
    }

    public async Task<Result<bool, Errors>> SaveChanges(CancellationToken cancellationToken)
    {
        var saveResult = await SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            return new Errors([GeneralErrors.Failure(saveResult.Error)]);
        }

        return saveResult.IsSuccess;
    }

    public async Task<Result<Department, Error>> GetByIdWithLocations(DepartmentId id,
        CancellationToken cancellationToken)
    {
        var department = await _context.Departments
            .Include(d => d.DepartmentLocations)
            .FirstOrDefaultAsync(d => d.Id == id && d.IsActive, cancellationToken);

        if (department is null)
        {
            return GeneralErrors.NotFound(id.Value, nameof(Department));
        }

        return department;
    }

    public Task<int> DeleteDepartmentLocations(
        DepartmentId departmentId,
        CancellationToken cancellationToken)
    {
        return _context.DepartmentLocations
            .Where(dl => dl.DepartmentId == departmentId)
            .ExecuteDeleteAsync(cancellationToken);
    }

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

    private sealed class DepartmentMoveSnapshotRow
    {
        public Guid Id { get; set; }

        public Guid? ParentId { get; set; }

        public string Identifier { get; set; } = string.Empty;

        public string Path { get; set; } = string.Empty;

        public int Depth { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }

    public Task<int> DeleteDepartmentLocation(
        DepartmentId departmentId,
        LocationId locationId,
        CancellationToken cancellationToken)
    {
        return _context.DepartmentLocations
            .Where(dl => dl.DepartmentId == departmentId && dl.LocationId == locationId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task AddDepartmentLocations(IEnumerable<DepartmentLocation> departmentLocations,
        CancellationToken cancellationToken)
    {
        await _context.DepartmentLocations.AddRangeAsync(departmentLocations, cancellationToken);
    }
    
    public async Task AddDepartmentLocations(DepartmentLocation departmentLocation,
        CancellationToken cancellationToken)
    {
        await _context.DepartmentLocations.AddAsync(departmentLocation, cancellationToken);
    }

    public async Task<Result<IReadOnlyList<Department>, Error>> GetByIdsWithPositions(List<Guid> ids,
        CancellationToken cancellationToken)
    {
        var departments = _context.Departments
            .Include(d => d.DepartmentPositions)
            .Where(d => ids.Contains(d.Id) && d.IsActive);

        if (!await departments.AnyAsync(cancellationToken))
        {
            var massingId = await departments.Select(d => d.Id.Value).ToListAsync(cancellationToken);
            return GeneralErrors.NotFound(massingId, nameof(Department));
        }

        return await departments.ToListAsync(cancellationToken);
    }

    public async Task<Result<bool, Error>> ExistsActiveDepartmentById(DepartmentId departmentId,
        CancellationToken cancellationToken)
    {
        var foundDepartment = await _context.Departments.AnyAsync(l => l.Id == departmentId && l.IsActive, cancellationToken);
        if (!foundDepartment)
        {
            return GeneralErrors.NotFound(departmentId, nameof(Department));
        }

        return foundDepartment;
    }

    public async Task<Result<bool>> ExistsLinkDepartmentAndLocation(DepartmentId departmentId, LocationId locationId,
        CancellationToken cancellationToken)
    {
        return await _context.DepartmentLocations.AnyAsync(
            dl => dl.DepartmentId == departmentId && dl.LocationId == locationId, cancellationToken);
    }

    public void Delete(Department department)
    {
        _context.Departments.Remove(department);
    }
    
    public void DeleteRange(IReadOnlyList<Department> departments)
    {
        _context.Departments.RemoveRange(departments);
    }

    public async Task<IReadOnlyList<Department>> GetExpiredSoftDeletedLeaves(DateTime expirationTime, int limit, CancellationToken cancellationToken)
    {
        return await _context.Departments
            .IgnoreQueryFilters()
            .Where(d => d.IsDeleted && d.SoftDeletedAt < expirationTime)
            .Where(d => !_context.Departments.IgnoreQueryFilters().Any(child => child.ParentId == d.Id))
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<Result<(Department Target, List<Department> Children), Error>> GetDepartmentWithChildren(
        DepartmentId departmentId, 
        CancellationToken cancellationToken)
    {
        var targetDepartment = await _context.Departments
            .FirstOrDefaultAsync(d => d.Id == departmentId, cancellationToken);

        if (targetDepartment is null)
        {
            return GeneralErrors.NotFound(departmentId.Value, nameof(Department));
        }
        
        var children = new List<Department>();
        await LoadChildrenRecursive(targetDepartment, children, cancellationToken);

        return (targetDepartment, children);
    }
    
    private async Task LoadChildrenRecursive(
        Department parent, 
        List<Department> allChildren, 
        CancellationToken cancellationToken)
    {
        await _context.Entry(parent)
            .Collection(d => d.DepartmentsChildren)
            .LoadAsync(cancellationToken);

        foreach (var child in parent.DepartmentsChildren)
        {
            allChildren.Add(child);
            await LoadChildrenRecursive(child, allChildren, cancellationToken);
        }
    }
}
