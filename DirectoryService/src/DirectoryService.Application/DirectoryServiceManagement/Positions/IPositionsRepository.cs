using System;
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
    
    Task<Result<bool, Error>> IsActivePositionByName(PositionName positionName, 
        CancellationToken cancellationToken);
}