using DirectoryService.Application.Database;
using DirectoryService.Application.DirectoryServiceManagement.Departments;
using DirectoryService.Application.DirectoryServiceManagement.Locations;
using DirectoryService.Application.DirectoryServiceManagement.Positions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure.BackgroundServices;

public class SoftDeletePurgeBackgroundService(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<SoftDeletePurgeBackgroundService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalSeconds = configuration.GetValue("SoftDeletePurge:IntervalSeconds", 3600);
        var interval = TimeSpan.FromSeconds(intervalSeconds);

        logger.LogInformation("SoftDeletePurgeBackgroundService started with run interval {IntervalSeconds}s.", intervalSeconds);

        using var timer = new PeriodicTimer(interval);

        do
        {
            try
            {
                await PurgeAllDeletedRecordsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred during soft-deleted records purging run.");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
    
    public async Task PurgeAllDeletedRecordsAsync(CancellationToken cancellationToken)
    {
        var expirationDays = configuration.GetValue("SoftDeletePurge:ExpirationDays", 30);
        var batchSize = configuration.GetValue("SoftDeletePurge:BatchSize", 1000);
        var expirationTime = DateTime.UtcNow.AddDays(-expirationDays);

        logger.LogInformation("Starting purge of soft-deleted records older than {ExpirationTime} (BatchSize: {BatchSize}).", expirationTime, batchSize);

        using var scope = scopeFactory.CreateScope();
        
        // 1. Очистка Локаций
        await PurgeEntityAsync(
            nameof(Location),
            cancellationToken,
            token => scope.ServiceProvider.GetRequiredService<ILocationsRepository>().GetExpiredSoftDeleted(expirationTime, batchSize, token),
            entity => scope.ServiceProvider.GetRequiredService<ILocationsRepository>().DeleteRange(entity),
            scope.ServiceProvider.GetRequiredService<ITransactionManager>(),
            batchSize);

        // 2. Очистка Позиций
        await PurgeEntityAsync(
            nameof(Position),
            cancellationToken,
            token => scope.ServiceProvider.GetRequiredService<IPositionsRepository>().GetExpiredSoftDeleted(expirationTime, batchSize, token),
            entity => scope.ServiceProvider.GetRequiredService<IPositionsRepository>().DeleteRange(entity),
            scope.ServiceProvider.GetRequiredService<ITransactionManager>(),
            batchSize);

        // 3. Очистка Департаментов (Листьев)
        await PurgeEntityAsync(
            nameof(Department),
            cancellationToken,
            token => scope.ServiceProvider.GetRequiredService<IDepartmentsRepository>().GetExpiredSoftDeletedLeaves(expirationTime, batchSize, token),
            entity => scope.ServiceProvider.GetRequiredService<IDepartmentsRepository>().DeleteRange(entity),
            scope.ServiceProvider.GetRequiredService<ITransactionManager>(),
            batchSize,
            continueIfProgressMade: true);
    }
    
    private async Task PurgeEntityAsync<TEntity>(
        string entityName,
        CancellationToken cancellationToken,
        Func<CancellationToken, Task<IReadOnlyList<TEntity>>> getExpiredFunc,
        Action<IReadOnlyList<TEntity>> deleteAction,
        ITransactionManager transactionManager,
        int batchSize,
        bool continueIfProgressMade = false)
    {
        int totalDeleted = 0;
        int batchCount;

        do
        {
            var records = await getExpiredFunc(cancellationToken);
            batchCount = records.Count;

            if (batchCount > 0)
            {
                deleteAction(records);
                    
                var saveResult = await transactionManager.SaveChangesAsync(cancellationToken);
                if (saveResult.IsFailure)
                {
                    throw new InvalidOperationException(
                        $"Failed to save deleted {entityName}: {saveResult.Error.Message}");
                }

                totalDeleted += batchCount;
            }
        } while ((continueIfProgressMade ? batchCount > 0 : batchCount >= batchSize) && !cancellationToken.IsCancellationRequested);

        if (totalDeleted > 0)
        {
            logger.LogInformation("Purged {Count} soft-deleted {EntityName}.", totalDeleted, entityName);
        }
    }
}
