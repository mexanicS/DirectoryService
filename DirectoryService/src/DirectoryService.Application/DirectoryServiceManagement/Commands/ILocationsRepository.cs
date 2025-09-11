using DirectoryService.Domain.Locations;

namespace DirectoryService.Application.DirectoryServiceManagement.Commands;

public interface ILocationsRepository
{
    Task<Guid> AddAsync(Location location, 
        CancellationToken cancellationToken = default);
    
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}