using CSharpFunctionalExtensions;

using DirectoryService.Application.Database;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace DirectoryService.Infrastructure.DataBase;

public class TransactionManager(
    DirectoryServiceDbContext dbContext,
    ILogger<TransactionManager> logger,
    ILoggerFactory loggerFactory)
    : ITransactionManager
{
    public async Task<Result<ITransactionScope, Error>> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        try
        {
            var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

            var transactionScopeLogger = loggerFactory.CreateLogger<TransactionScope>();

            var transactionScope = new TransactionScope(transaction.GetDbTransaction(), transactionScopeLogger);

            return transactionScope;
        }
        catch (Exception e)
        {
            var message = "Failed to begin transaction.";

            logger.LogError(e, message);

            return Error.Failure("database", message);
        }
    }

    public async Task<UnitResult<Error>> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);

            return UnitResult.Success<Error>();
        }
        catch (Exception e)
        {
            var message = "Failed to save changes.";

            logger.LogError(e, message);

            return Error.Failure("database", message);
        }
    }
}