using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure.Repositories;

public abstract class BaseRepository<T> 
    where T : class
{
    private readonly DirectoryServiceDbContext _context;
    private readonly ILogger _logger;

    protected BaseRepository(DirectoryServiceDbContext context, ILogger logger)
    {
        _context = context;
        _logger = logger;
    }

    protected async Task<Result> SaveChangesAsync(CancellationToken cancellationToken = default)
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
    
    protected static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        return ex.InnerException is Npgsql.PostgresException { SqlState: Npgsql.PostgresErrorCodes.UniqueViolation };
    }
}