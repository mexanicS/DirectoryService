using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.Locations;
using SharedKernel;

namespace DirectoryService.Application.DirectoryServiceManagement.Locations;

public interface ILocationsRepository
{
    Task<Result<Guid, Error>> AddAsync(Location location, 
        CancellationToken cancellationToken = default);
    
    Task<Result<bool, Error>> ExistsByAddressAsync(Address address, CancellationToken cancellationToken);
    
    Task<Result<bool, Error>> ExistsActiveLocationById(LocationId locationId, CancellationToken cancellationToken);
    
    Task<Result<bool, Error>> ExistsActiveLocationsById(IEnumerable<Guid> locationsId, CancellationToken cancellationToken);
}