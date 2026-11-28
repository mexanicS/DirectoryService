using Amazon.Runtime;
using Amazon.S3;
using FileService.Core.ObjectStorage;
using FileService.Infrastructure.S3.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace FileService.Infrastructure.S3;

public static class Inject
{
    internal const string OPERATIONS_CLIENT = "s3-operations";
    internal const string PRESIGNING_CLIENT = "s3-presigning";

    public static IServiceCollection AddS3Storage(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IValidateOptions<S3Options>, S3OptionsValidator>();
        services.AddOptions<S3Options>()
            .Bind(configuration.GetSection(S3Options.SECTION_NAME))
            .ValidateOnStart();

        services.TryAddSingleton(TimeProvider.System);
        services.AddKeyedSingleton<IAmazonS3>(OPERATIONS_CLIENT, (provider, _) =>
        {
            var options = provider.GetRequiredService<IOptions<S3Options>>().Value;
            return CreateClient(options, options.Endpoint);
        });
        services.AddKeyedSingleton<IAmazonS3>(PRESIGNING_CLIENT, (provider, _) =>
        {
            var options = provider.GetRequiredService<IOptions<S3Options>>().Value;
            return CreateClient(options, options.PublicEndpoint);
        });

        services.AddSingleton<S3ObjectStorage>();
        services.AddSingleton<IObjectStorage>(provider => provider.GetRequiredService<S3ObjectStorage>());
        services.AddHostedService<S3BucketInitializer>();
        services.AddHealthChecks().AddCheck<S3HealthCheck>("s3", tags: ["ready"]);
        return services;
    }

    private static AmazonS3Client CreateClient(S3Options options, string endpoint) =>
        new(
            new BasicAWSCredentials(options.AccessKey, options.SecretKey),
            new AmazonS3Config
            {
                ServiceURL = endpoint,
                AuthenticationRegion = options.Region,
                ForcePathStyle = options.ForcePathStyle,
                MaxErrorRetry = 0,
                Timeout = options.RequestTimeout,
            });
}