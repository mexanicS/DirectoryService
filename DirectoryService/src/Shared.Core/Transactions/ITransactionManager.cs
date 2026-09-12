using System.Data;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using SharedKernel;

namespace Shared.Core.Transactions;

public interface ITransactionManager
{
    Task<Result<ITransactionScope, Error>> BeginTransactionAsync(
        CancellationToken cancellationToken,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

    Task<UnitResult<Error>> SaveChangesAsync(CancellationToken cancellationToken);
}
