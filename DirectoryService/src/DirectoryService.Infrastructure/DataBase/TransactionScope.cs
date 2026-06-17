using System.Data;
using CSharpFunctionalExtensions;
using DirectoryService.Application.DataBase;
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
}