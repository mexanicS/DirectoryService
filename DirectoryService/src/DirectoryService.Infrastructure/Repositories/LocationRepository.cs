using CSharpFunctionalExtensions;
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

    public async Task<Result> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}