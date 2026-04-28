using System.Data;
using CSharpFunctionalExtensions;
using DirectoryService.Application.DataBase;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace DirectoryService.Infrastructure.DataBase;

public class TransactionScope : ITransactionScope
{
    private readonly IDbTransaction _transaction;
    private readonly ILogger<TransactionScope> _logger;

    public TransactionScope(IDbTransaction transaction, ILogger<TransactionScope> logger)
    {
        _transaction = transaction;
        _logger = logger;
    }

    public UnitResult<Error> Commit()
    {
        try
        {
            _transaction.Commit();

            return UnitResult.Success<Error>();
        }
        catch (Exception e)
        {
            var message = "Failed to commit transaction.";

            _logger.LogError(e, message);

            return Error.Failure("transaction.commit.failure", message);
        }
    }

    public UnitResult<Error> Rollback()
    {
        try
        {
            _transaction.Rollback();

            return UnitResult.Success<Error>();
        }
        catch (Exception e)
        {
            var message = "Failed to rollback transaction.";

            _logger.LogError(e, message);

            return Error.Failure("transaction.rollback.failure", message);
        }
    }
}