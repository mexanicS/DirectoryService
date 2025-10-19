using CSharpFunctionalExtensions;
using DirectoryService.Domain.Locations;
using SharedKernel;

namespace DirectoryService.Application.DirectoryServiceManagement.Commands;

public interface ILocationsRepository
{
    Task<Result<Guid, Errors>> AddAsync(Location location, 
        CancellationToken cancellationToken = default);
    
    Task<Result> SaveChangesAsync(CancellationToken cancellationToken = default);
    
    Task<Result<bool, Error>> ExistsByAddressAsync(Address address, CancellationToken cancellationToken);
}