using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;

namespace DirectoryService.Domain.DepartmentLocations;

public sealed class DepartmentLocation
{
    public DepartmentLocationId Id { get; init; }
    
    public LocationId  LocationId { get; init; }
    
    public DepartmentId  DepartmentId { get; init; }
    
    public static Result<DepartmentLocation> Create(DepartmentId departmentId, LocationId locationId)
    {
        return new DepartmentLocation(departmentId, locationId, DepartmentLocationId.Create(Guid.NewGuid()));
    }
    
    private DepartmentLocation(DepartmentId departmentId, LocationId locationId, DepartmentLocationId id)
    {
        DepartmentId = departmentId;
        LocationId = locationId;
        Id = id;
    }
}