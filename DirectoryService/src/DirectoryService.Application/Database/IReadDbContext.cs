using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;

namespace DirectoryService.Application.DataBase;

public interface IReadDbContext
{
    IQueryable<Department> Departments { get; }

    IQueryable<Location> Locations { get; }

    IQueryable<Position> Positions { get; }
    
    IQueryable<DepartmentLocation> DepartmentLocations { get; }

    IQueryable<DepartmentPosition> DepartmentPositions { get; }

}
