using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.Positions;
using SharedKernel;

namespace DirectoryService.Application.DirectoryServiceManagement.Positions;

public interface IPositionsRepository
{
    Task<Result<Guid, Errors>> Add(Position position,
        CancellationToken cancellationToken);

    Task<Result<bool, Error>> IsActivePositionByName(
        PositionName positionName,
        CancellationToken cancellationToken,
        Guid? excludePositionId = null);

    Task<Result<Position, Error>> GetById(Guid positionId, 
        CancellationToken cancellationToken);

    void Delete(Position position);

    void DeleteRange(IReadOnlyList<Position> positions);

    Task<IReadOnlyList<Position>> GetExpiredSoftDeleted(DateTime expirationTime, int limit, CancellationToken cancellationToken);
}