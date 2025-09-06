using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;

namespace DirectoryService.Domain.DepartmentLocations;

public sealed class DepartmentLocation
{
    public DepartmentLocationId Id { get; init; }
    
    public LocationId  LocationId { get; init; }
    
    public DepartmentId  DepartmentId { get; init; }
}