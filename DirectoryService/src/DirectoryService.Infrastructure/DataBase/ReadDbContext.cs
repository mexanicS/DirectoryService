using DirectoryService.Application.Database;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Infrastructure.DataBase;

public class ReadDbContext(DirectoryServiceDbContext context) : IReadDbContext
{
    public IQueryable<Department> Departments => context.Departments.AsNoTracking();

    public IQueryable<Location> Locations => context.Locations.AsNoTracking();

    public IQueryable<Position> Positions => context.Positions.AsNoTracking();
    
    public IQueryable<DepartmentLocation> DepartmentLocations => context.DepartmentLocations.AsNoTracking();
    
    public IQueryable<DepartmentPosition> DepartmentPositions => context.DepartmentPositions.AsNoTracking();
}
