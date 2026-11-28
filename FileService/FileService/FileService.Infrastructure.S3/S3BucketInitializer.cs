using FileService.Core.ObjectStorage;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FileService.Infrastructure.S3;

public class S3BucketInitializer(
    IObjectStorage storage,
    ILogger<S3BucketInitializer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var retrySeconds = 1;

        while (!stoppingToken.IsCancellationRequested)
        {
            var result = await storage.EnsureBucketAsync(stoppingToken);
            if (result.IsSuccess)
            {
                logger.LogInformation("Object storage bucket is initialized");
                return;
            }

            logger.LogWarning(
                "Object storage initialization failed ({ErrorCode}); retrying in {RetrySeconds} seconds",
                result.Error.Code,
                retrySeconds);
            await Task.Delay(TimeSpan.FromSeconds(retrySeconds), stoppingToken);
            retrySeconds = Math.Min(retrySeconds * 2, 30);
        }
    }
}