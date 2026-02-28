using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using SharedKernel;

namespace DirectoryService.Application.DirectoryServiceManagement.Departments;

public interface IDepartmentsRepository
{
    Task<Result<Guid, Errors>> Add(Department department, CancellationToken cancellationToken);
    
    Task<Result<Department, Error>> GetById(Guid parentId, CancellationToken cancellationToken);
    
    Task<Result<Department, Error>> GetByIdWithPositions(DepartmentId id, CancellationToken cancellationToken);
    
    Task<Result<bool, Error>> DepartmentExists(IEnumerable<Guid> departmentIds, 
        CancellationToken cancellationToken);
    
    Task<Result<bool, Errors>> Save(CancellationToken cancellationToken);
}