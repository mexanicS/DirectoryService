using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using DirectoryService.Application.DirectoryServiceManagement.Departments;
using DirectoryService.Domain.Departments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace DirectoryService.Infrastructure.Repositories;

public class DepartmentRepository : BaseRepository<Department>, IDepartmentsRepository
{
    private readonly DirectoryServiceDbContext _context;
    private readonly ILogger<DepartmentRepository> _logger;

    public DepartmentRepository(DirectoryServiceDbContext context,
        ILogger<DepartmentRepository> logger) : base(context, logger)
    {
        _context = context;
        _logger = logger;
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
            _logger.LogWarning(ex, "Location with this name already exists");
            return GeneralErrors.AlreadyExist().ToErrors();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error to add department");
            return GeneralErrors.Failure().ToErrors();
        }
    }

    public async Task<Result<Department, Error>> GetById(Guid parentId, CancellationToken cancellationToken)
    {
        var department =
            await _context.Departments.FirstOrDefaultAsync(d => d.Id == parentId && d.IsActive, cancellationToken);

        if (department == null)
        {
            return GeneralErrors.NotFound(parentId, nameof(Department));
        }

        return department;
    }
    
    public async Task<Result<bool, Error>> DepartmentExists(IEnumerable<Guid> departmentIds,
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

    public async Task<Result<Department, Error>> GetByIdWithPositions(DepartmentId id, CancellationToken cancellationToken)
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
}