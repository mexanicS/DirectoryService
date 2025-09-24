using CSharpFunctionalExtensions;
using DirectoryService.Application.DirectoryServiceManagement.Commands;
using DirectoryService.Domain.Locations;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure.Repositories;

public class LocationsRepository : ILocationsRepository
{
    private readonly DirectoryServiceDbContext _context;
    private readonly ILogger<LocationsRepository> _logger;

    public LocationsRepository(DirectoryServiceDbContext context,
        ILogger<LocationsRepository> logger)
    {
        _context = context;
        _logger = logger;
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
            _logger.LogCritical(ex, "An error when trying to save changes in the database");
            return Result.Failure(ex.Message);
        }
    }
}