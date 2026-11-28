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
        var initialized = false;

        while (!stoppingToken.IsCancellationRequested)
        {
            var result = await storage.EnsureBucketAsync(stoppingToken);
            if (result.IsSuccess)
            {
                if (!initialized)
                {
                    logger.LogInformation("Object storage bucket is initialized");
                }

                initialized = true;
                retrySeconds = 1;
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                continue;
            }

            initialized = false;
            logger.LogWarning(
                "Object storage initialization failed ({ErrorCode}); retrying in {RetrySeconds} seconds",
                result.Error.Code,
                retrySeconds);
            await Task.Delay(TimeSpan.FromSeconds(retrySeconds), stoppingToken);
            retrySeconds = Math.Min(retrySeconds * 2, 30);
        }
    }
}