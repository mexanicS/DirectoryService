using DirectoryService.Application.DataBase;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Infrastructure.DataBase;

public class ReadDbContext : IReadDbContext
{
    private readonly DirectoryServiceDbContext _context;

    public ReadDbContext(DirectoryServiceDbContext context)
    {
        _context = context;
    }

    public IQueryable<Department> Departments => _context.Departments.AsNoTracking();

    public IQueryable<Location> Locations => _context.Locations.AsNoTracking();

    public IQueryable<Position> Positions => _context.Positions.AsNoTracking();
}
