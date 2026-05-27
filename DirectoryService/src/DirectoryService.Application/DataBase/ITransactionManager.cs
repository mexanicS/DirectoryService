using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using SharedKernel;

namespace DirectoryService.Application.DataBase;

public interface ITransactionManager
{
    Task<Result<ITransactionScope, Error>> BeginTransactionAsync(CancellationToken cancellationToken);

    Task<UnitResult<Error>> SaveChangesAsync(CancellationToken cancellationToken);
}