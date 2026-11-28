using FileService.Core.ObjectStorage;
using FileService.Domain.StorageReferences;
using FileService.Infrastructure.S3;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace FileService.IntegrationTests;

[Trait("Category", "S3")]
public class S3AvailabilityTests
{
    [Fact]
    public async Task Host_StartsWhileStorageIsUnresponsiveAndRecoversWhenItReturns()
    {
        var fixture = new MinioFixture();
        var paused = false;
        try
        {
            await fixture.InitializeAsync();
            var options = fixture.CreateOptions();
            options.RequestTimeout = TimeSpan.FromSeconds(1);
            options.HealthCheckTimeout = TimeSpan.FromMilliseconds(250);
            await fixture.PauseAsync();
            paused = true;
            using var host = new HostBuilder()
                .ConfigureServices(services => services.AddS3Storage(MinioFixture.CreateConfiguration(options)))
                .Build();

            await host.StartAsync().WaitAsync(TimeSpan.FromSeconds(5));
            var healthChecks = host.Services.GetRequiredService<HealthCheckService>();
            Assert.Equal(HealthStatus.Unhealthy, (await healthChecks.CheckHealthAsync()).Status);

            await fixture.ResumeAsync();
            paused = false;
            Assert.Equal(options.Endpoint, fixture.Endpoint);
            await WaitForHealthyAsync(healthChecks);

            await fixture.PauseAsync();
            paused = true;
            Assert.Equal(HealthStatus.Unhealthy, (await healthChecks.CheckHealthAsync()).Status);
            await fixture.ResumeAsync();
            paused = false;
            await WaitForHealthyAsync(healthChecks);
            await host.StopAsync();
        }
        finally
        {
            if (paused)
            {
                await fixture.ResumeAsync();
            }

            await fixture.DisposeAsync();
        }
    }

    [Fact]
    public async Task Storage_WhenUnresponsive_ReportsTimeoutAndPropagatesCallerCancellation()
    {
        var fixture = new MinioFixture();
        var paused = false;
        try
        {
            await fixture.InitializeAsync();
            var options = fixture.CreateOptions();
            options.RequestTimeout = TimeSpan.FromSeconds(1);
            options.HealthCheckTimeout = TimeSpan.FromMilliseconds(250);
            using var services = fixture.CreateServices(options);
            var storage = services.GetRequiredService<IObjectStorage>();
            var key = StorageKey.Create("originals/unresponsive-object").Value;
            await fixture.PauseAsync();
            paused = true;

            var timedOut = await storage.GetMetadataAsync(key, default).WaitAsync(TimeSpan.FromSeconds(5));
            Assert.True(timedOut.IsFailure);
            Assert.Equal("storage.unavailable", timedOut.Error.Code);

            using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                storage.GetMetadataAsync(key, cancellation.Token).WaitAsync(TimeSpan.FromSeconds(5)));
            Assert.True(cancellation.IsCancellationRequested);
        }
        finally
        {
            if (paused)
            {
                await fixture.ResumeAsync();
            }

            await fixture.DisposeAsync();
        }
    }

    private static async Task WaitForHealthyAsync(HealthCheckService healthChecks)
    {
        using var recoveryTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(45));
        while (true)
        {
            var report = await healthChecks.CheckHealthAsync(cancellationToken: recoveryTimeout.Token);
            if (report.Status == HealthStatus.Healthy)
            {
                return;
            }

            await Task.Delay(200, recoveryTimeout.Token);
        }
    }
}