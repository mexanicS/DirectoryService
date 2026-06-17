using CSharpFunctionalExtensions;
using DirectoryService.Application.DirectoryServiceManagement.Departments;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace DirectoryService.Infrastructure.Repositories;

public class DepartmentRepository(
    DirectoryServiceDbContext context,
    ILogger<DepartmentRepository> logger)
    : BaseRepository<Department>(context, logger), IDepartmentsRepository
{
    private readonly DirectoryServiceDbContext _context = context;

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

    public async Task DeleteLocationsByDepartmentId(Guid id, CancellationToken cancellationToken)
    {
        await _context.DepartmentLocations
            .Where(dl => dl.DepartmentId == id)
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
    
    public async Task<Result<(Department Target, List<Department> Children), Error>> GetDepartmentWithChildren(
        DepartmentId departmentId, 
        CancellationToken cancellationToken)
    {
        // 1. Загружаем целевой отдел
        var targetDepartment = await _context.Departments
            .FirstOrDefaultAsync(d => d.Id == departmentId, cancellationToken);

        if (targetDepartment is null)
        {
            return GeneralErrors.NotFound(departmentId.Value, nameof(Department));
        }

        // 2. Рекурсивно загружаем все дочерние элементы в память контекста EF
        // Мы просто обходим дерево вниз. EF Core автоматически начнет отслеживать (track) их все!
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