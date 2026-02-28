using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using DirectoryService.Application.DirectoryServiceManagement.Positions;
using DirectoryService.Domain.Positions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace DirectoryService.Infrastructure.Repositories;

public class PositionRepository : BaseRepository<Position>, IPositionsRepository
{
    private readonly DirectoryServiceDbContext _context;
    private readonly ILogger _logger;

    public PositionRepository(DirectoryServiceDbContext context, 
        ILogger logger) 
        : base(context, logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<Result<Guid, Errors>> Add(Position position,
        CancellationToken cancellationToken)
    {
        try
        {
            await _context.Positions.AddAsync(position, cancellationToken);
            var saveResult = await SaveChangesAsync(cancellationToken);
            if (saveResult.IsFailure)
            {
                return new Errors([GeneralErrors.Failure(saveResult.Error)]);
            }

            return position.Id.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error to add position");
            return GeneralErrors.Failure().ToErrors();
        }
    }
    

    public async Task<Result<bool, Error>> IsActivePositionByName(PositionName positionName,
        CancellationToken cancellationToken)
    {
        var positionExists = await _context.Positions
            .AnyAsync(p => p.Name == positionName && p.IsActive, cancellationToken);
        
        return positionExists;
    }
}