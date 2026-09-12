using CSharpFunctionalExtensions;

using SharedKernel;

namespace Shared.Core.Transactions;

public interface ITransactionScope : IDisposable
{
    UnitResult<Error> Commit();

    UnitResult<Error> Rollback();
}
