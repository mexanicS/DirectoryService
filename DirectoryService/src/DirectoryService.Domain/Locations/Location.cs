using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentLocations;

namespace DirectoryService.Domain.Locations;

public sealed class Location
{
    //EF Core
    private Location()
    {
    }
    
    private readonly List<DepartmentLocation> _departmentLocations = [];
    
    public Location(
        LocationId  locationId,
        LocationName name, 
        Address address, 
        Timezone timezone)
    {
        Id = locationId;
        Name = name;
        Address = address;
        Timezone = timezone;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }
    
    public LocationId Id { get; private set; }
     
    public LocationName Name { get; private set; }
    
    public Address Address { get; private set; }

    public Timezone Timezone { get; private set; }
    
    public bool IsActive { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
     
    public DateTime UpdatedAt { get; private set; }
    
    public IReadOnlyList<DepartmentLocation> DepartmentLocations => _departmentLocations;

    public static Result<Location> Create(
        LocationId  locationId,
        LocationName name, 
        Address address, 
        Timezone timezone)
    {
        return new Location(locationId, name, address, timezone);
    }
}