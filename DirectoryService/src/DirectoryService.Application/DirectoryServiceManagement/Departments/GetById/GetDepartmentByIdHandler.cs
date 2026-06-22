using CSharpFunctionalExtensions;

using DirectoryService.Application.Database;
using DirectoryService.Contract;
using DirectoryService.Domain.Departments;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace DirectoryService.Application.DirectoryServiceManagement.Departments.GetById;

public class GetDepartmentByIdHandler(IReadDbContext readDb)
{
    public async Task<Result<DepartmentResponse, Error>> Handle(
        GetDepartmentByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var response = await readDb.Departments
            .Where(d => d.Id == query.Id && d.IsActive)
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
            .FirstOrDefaultAsync(cancellationToken);

        if (response is null)
            return GeneralErrors.NotFound(query.Id, nameof(Department));

        return response;
    }
}
