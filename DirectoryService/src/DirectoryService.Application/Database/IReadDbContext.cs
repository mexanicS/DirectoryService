using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;

namespace DirectoryService.Application.DataBase;

public interface IReadDbContext
{
    IQueryable<Department> Departments { get; }

    IQueryable<Location> Locations { get; }

    IQueryable<Position> Positions { get; }
}
