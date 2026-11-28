using FileService.Infrastructure.S3.Options;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace FileService.Infrastructure.S3;

public class S3HealthCheck(
    S3ObjectStorage storage,
    IOptions<S3Options> options) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(options.Value.HealthCheckTimeout);

        try
        {
            var result = await storage.CheckBucketAsync(timeout.Token);
            return result.IsSuccess
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy(result.Error.Code);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return HealthCheckResult.Unhealthy("Object storage check timed out.");
        }
    }
}