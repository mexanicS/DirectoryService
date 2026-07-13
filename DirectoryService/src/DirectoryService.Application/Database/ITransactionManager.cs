using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using SharedKernel;

namespace DirectoryService.Application.Database;

public interface ITransactionManager
{
    Task<Result<ITransactionScope, Error>> BeginTransactionAsync(CancellationToken cancellationToken);

    Task<UnitResult<Error>> SaveChangesAsync(CancellationToken cancellationToken);
}