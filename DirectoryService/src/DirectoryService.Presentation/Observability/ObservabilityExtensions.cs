using DirectoryService.Infrastructure.Observability;

using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

namespace DirectoryService.Presentation.Observability;

public static class ObservabilityExtensions
{
    public static IServiceCollection AddDirectoryServiceObservability(
        this IServiceCollection services,
        IHostEnvironment environment)
    {
        services
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(
                    DirectoryServiceMetrics.MeterName,
                    serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString())
                .AddAttributes(
                [
                    new KeyValuePair<string, object>("deployment.environment.name", environment.EnvironmentName),
                ]))
            .WithMetrics(metrics => metrics
                .AddMeter(
                    DirectoryServiceMetrics.MeterName,
                    "Microsoft.AspNetCore.Hosting",
                    "Microsoft.AspNetCore.Server.Kestrel",
                    "Microsoft.EntityFrameworkCore",
                    "Npgsql",
                    "System.Runtime")
                .AddView(
                    "*",
                    new MetricStreamConfiguration
                    {
                        ExcludedTagKeys = ["db.client.connection.pool.name"],
                    })
                .AddPrometheusExporter());

        return services;
    }

    public static WebApplication MapDirectoryServiceMetrics(this WebApplication app)
    {
        if (app.Configuration.GetValue<bool>("Observability:Prometheus:EndpointEnabled"))
        {
            app.MapPrometheusScrapingEndpoint().DisableHttpMetrics();
        }

        return app;
    }
}
