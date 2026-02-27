using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using SharedKernel;

namespace DirectoryService.Application.DirectoryServiceManagement.Departments;

public interface IDepartmentsRepository
{
    Task<Result<Guid, Errors>> Add(Department department, CancellationToken cancellationToken);
    
    Task<Result<Department, Error>> GetById(Guid parentId, CancellationToken cancellationToken);
}