using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using DirectoryService.Application.DirectoryServiceManagement.Locations;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace DirectoryService.Infrastructure.Repositories;

public class LocationsRepository : BaseRepository<Location>, ILocationsRepository
{
    private readonly DirectoryServiceDbContext _context;
    private readonly ILogger<LocationsRepository> _logger;

    public LocationsRepository(DirectoryServiceDbContext context,
        ILogger<LocationsRepository> logger) : base(context, logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<Result<Guid, Errors>> AddAsync(Location location, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.Locations.AddAsync(location, cancellationToken);
            var saveResult = await SaveChangesAsync(cancellationToken);
            if (saveResult.IsFailure)
            {
                return new Errors([GeneralErrors.Failure(saveResult.Error)]);
            }
            return location.Id.Value;
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            _logger.LogWarning(ex, "Location with this name already exists");
            return GeneralErrors.AlreadyExist().ToErrors();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error to add location");
            return GeneralErrors.Failure().ToErrors();
        }
    }

    public async Task<Result<bool, Error>> ExistsByAddressAsync(Address address, CancellationToken cancellationToken)
    {
        var exists = await _context.Locations.AnyAsync(
            l =>
                l.Address.City == address.City &&
                l.Address.Street == address.Street &&
                l.Address.HouseNumber == address.HouseNumber &&
                l.Address.ZipCode == address.ZipCode,
            cancellationToken);
        
        return exists;
    }

    public async Task<Result<bool, Error>> ExistsActiveLocationById(LocationId locationId, CancellationToken cancellationToken)
    {
        return await _context.Locations.AnyAsync(l => l.Id == locationId && l.IsActive, cancellationToken);
    }

    public async Task<Result<bool, Error>> ExistsActiveLocationsById(IEnumerable<Guid> locationsId,
        CancellationToken cancellationToken)
    {
        var locationIds = locationsId.Distinct().ToArray();
        var expectedCount = locationIds.Length;

        var actualCount = await _context.Locations
            .CountAsync(l => locationIds.Contains(l.Id) && l.IsActive, cancellationToken);

        return expectedCount == actualCount
            ? true
            : Error.NotFound("location.id", $"Found {actualCount}/{expectedCount} locations");
    }
}