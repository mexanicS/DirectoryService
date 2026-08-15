using System.Data;
using System.Data.Common;
using CSharpFunctionalExtensions;

using DirectoryService.Application.Database;

using Microsoft.Extensions.Logging;
using SharedKernel;

namespace DirectoryService.Infrastructure.DataBase;

public class TransactionScope(IDbTransaction transaction, ILogger<TransactionScope> logger)
    : ITransactionScope
{
    public UnitResult<Error> Commit()
    {
        try
        {
            transaction.Commit();

            return UnitResult.Success<Error>();
        }
        catch (Exception e) when (IsTransactionConflict(e))
        {
            const string message = "Transaction conflicted with another database transaction.";

            logger.LogWarning(e, message);

            return Error.Conflict("transaction.conflict", message);
        }
        catch (Exception e)
        {
            var message = "Failed to commit transaction.";

            logger.LogError(e, message);

            return Error.Failure("transaction.commit.failure", message);
        }
    }

    public UnitResult<Error> Rollback()
    {
        try
        {
            transaction.Rollback();

            return UnitResult.Success<Error>();
        }
        catch (Exception e)
        {
            var message = "Failed to rollback transaction.";

            logger.LogError(e, message);

            return Error.Failure("transaction.rollback.failure", message);
        }
    }
    
    public void Dispose()
    {
        transaction.Dispose();
    }

    private static bool IsTransactionConflict(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is DbException { SqlState: { } sqlState } &&
                (sqlState.StartsWith("40", StringComparison.Ordinal) || sqlState == "55P03"))
            {
                return true;
            }
        }

        return false;
    }
}
