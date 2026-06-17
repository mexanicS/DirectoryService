using CSharpFunctionalExtensions;
using DirectoryService.Application.DataBase;
using DirectoryService.Contract;
using DirectoryService.Domain.Departments;

using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace DirectoryService.Application.DirectoryServiceManagement.Departments.Get;

public class GetDepartmentsHandler
{
    private readonly IReadDbContext _readDb;

    public GetDepartmentsHandler(IReadDbContext readDb)
    {
        _readDb = readDb;
    }
    public async Task<Result<PagedResult<DepartmentResponse>, Error>> Handle(
        GetDepartmentsQuery request,
        CancellationToken cancellationToken = default)
    {
        var query = _readDb.Departments;

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchPattern = $"%{request.Search}%";
            query = query.Where(d => EF.Functions.Like(d.Name, searchPattern));
        }

        int totalCount = await query.CountAsync(cancellationToken);

        query = ApplySorting(query, request.SortBy, request.SortDir);
        
        var page = request.Pagination.Page;
        var pageSize = request.Pagination.PageSize;

        var items = await query
            .Skip((request.Pagination.Page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new DepartmentResponse(
                    d.Id.Value,
                    d.Name.Value,
                    d.Identifier.Value,
                    d.Path.Value,
                    d.Depth.Value,
                    d.ParentId != null ? d.ParentId.Value : null,
                    d.IsActive,
                    d.CreatedAt,
                    d.UpdatedAt))
            .ToListAsync(cancellationToken);

        var result = new PagedResult<DepartmentResponse>(items, totalCount, page, pageSize);
        return Result.Success<PagedResult<DepartmentResponse>, Error>(result);
    }
    
    private IQueryable<Department> ApplySorting(IQueryable<Department> query, string? sortBy, string? sortDir)
    {
        var isDescending = sortDir?.Equals("desc", StringComparison.OrdinalIgnoreCase) ?? false;

        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return query.OrderByDescending(d => d.CreatedAt);
        }

        return sortBy.ToLower() switch
        {
            "name" => isDescending ? query.OrderByDescending(d => d.Name) : query.OrderBy(d => d.Name),
            "createdat" => isDescending ? query.OrderByDescending(d => d.CreatedAt) : query.OrderBy(d => d.CreatedAt),
            _ => query.OrderByDescending(d => d.CreatedAt)
        };
    }
}