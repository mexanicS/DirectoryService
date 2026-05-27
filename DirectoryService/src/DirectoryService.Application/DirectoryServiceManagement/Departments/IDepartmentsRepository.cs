using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using SharedKernel;

namespace DirectoryService.Application.DirectoryServiceManagement.Departments;

public interface IDepartmentsRepository
{
    Task<Result<Guid, Errors>> Add(Department department, CancellationToken cancellationToken);

    Task<Result<Department, Error>> GetById(Guid parentId, CancellationToken cancellationToken);

    Task<Result<Department, Error>> GetByIdWithPositions(DepartmentId id, CancellationToken cancellationToken);

    Task<Result<bool, Error>> DepartmentsExists(IEnumerable<Guid> departmentIds,
        CancellationToken cancellationToken);

    Task<Result<bool, Errors>> SaveChanges(CancellationToken cancellationToken);

    Task<Result<Department, Error>> GetByIdWithLocations(DepartmentId id,
        CancellationToken cancellationToken);

    Task DeleteLocationsByDepartmentId(Guid id, CancellationToken cancellationToken);

    Task AddDepartmentLocations(IEnumerable<DepartmentLocation> departmentLocations,
        CancellationToken cancellationToken);

    Task AddDepartmentLocations(DepartmentLocation departmentLocation,
        CancellationToken cancellationToken);
    
    Task<Result<IReadOnlyList<Department>, Error>> GetByIdsWithPositions(List<Guid> ids,
        CancellationToken cancellationToken);

    Task<Result<bool, Error>> ExistsActiveDepartmentById(DepartmentId departmentId, CancellationToken cancellationToken);

    Task<Result<bool, Error>> LinkDepartmentAndLocation(DepartmentId departmentId, LocationId locationId,
        CancellationToken cancellationToken);
}