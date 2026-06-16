using CSharpFunctionalExtensions;
using DirectoryService.Application.DataBase;
using DirectoryService.Application.DirectoryServiceManagement.DTOs;
using DirectoryService.Contract;
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

    public async Task<Result<GetDepartmentsDto, Error>> Handle(GetDepartmentsQuery  request,
        CancellationToken cancellationToken = default)
    {
        var departmentsQuery = _readDb.Departments;
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            departmentsQuery = departmentsQuery.Where(d => d.Name.ToString().Contains(request.Search, StringComparison.CurrentCultureIgnoreCase));
        }
        
        var departments = await departmentsQuery
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
        
    }
}