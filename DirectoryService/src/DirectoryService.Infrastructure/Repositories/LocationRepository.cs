using DirectoryService.Application.DirectoryServiceManagement.Commands;
using DirectoryService.Domain.Locations;

namespace DirectoryService.Infrastructure.Repositories;

public class LocationsRepository : ILocationsRepository
{
    private readonly DirectoryServiceDbContext _context;

    public LocationsRepository(DirectoryServiceDbContext context)
    {
        _context = context;
    }
    
    public async Task<Guid> AddAsync(Location location, 
        CancellationToken cancellationToken = default)
    {
        await _context.Locations.AddAsync(location,cancellationToken);
        
        return location.Id;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
         await _context.SaveChangesAsync(cancellationToken);
    }
}