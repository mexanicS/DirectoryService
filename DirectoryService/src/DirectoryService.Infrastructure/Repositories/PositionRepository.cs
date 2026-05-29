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
    private readonly ILogger<PositionRepository> _logger;

    public PositionRepository(DirectoryServiceDbContext context, 
        ILogger<PositionRepository> logger) 
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
            await _context.SaveChangesAsync(cancellationToken);
            
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
    
    public async Task<Result<Position, Error>> GetById(Guid positionId, CancellationToken cancellationToken)
    {
        var position = await _context.Positions
            .FirstOrDefaultAsync(d => d.Id == positionId && d.IsActive, cancellationToken);

        if (position is null)
        {
            return GeneralErrors.NotFound(positionId, nameof(Position));
        }

        return position;
    }

    public void Delete(Position position)
    {
        _context.Positions.Remove(position);
    }
}