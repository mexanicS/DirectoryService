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
    
    public async Task<Result<Guid>> AddAsync(Location location, 
        CancellationToken cancellationToken = default)
    {
        await _context.Locations.AddAsync(location, cancellationToken);

        return Result.Success<Guid>(location.Id).Value;
    }

    public async Task<Result> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            _logger.LogWarning(ex, "Location with this name already exists.");
            return Result.Failure("Location with this name already exists.");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "An error when trying to save changes in the database");
            return Result.Failure($"Database error: {ex.Message}");
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

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        return ex.InnerException is Npgsql.PostgresException { SqlState: "23505" };
    }
}