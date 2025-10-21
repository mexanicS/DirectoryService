using CSharpFunctionalExtensions;
using DirectoryService.Application.DirectoryServiceManagement.Commands;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

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
    
    public async Task<Result<Guid, Errors>> AddAsync(Location location, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.Locations.AddAsync(location, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

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
        bool exists = await _context.Locations.AnyAsync(
            l =>
                l.Address.City == address.City &&
                l.Address.Street == address.Street &&
                l.Address.HouseNumber == address.HouseNumber &&
                l.Address.ZipCode == address.ZipCode,
            cancellationToken);
        
        return exists;
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
            return Result.Failure($"Database error: {ex.Message}");
        }
        
    }
    
    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        return ex.InnerException is Npgsql.PostgresException { SqlState: Npgsql.PostgresErrorCodes.UniqueViolation };
    }
}