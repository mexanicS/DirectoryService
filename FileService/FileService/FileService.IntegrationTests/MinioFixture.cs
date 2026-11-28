using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using FileService.Infrastructure.S3;
using FileService.Infrastructure.S3.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FileService.IntegrationTests;

public class MinioFixture : IAsyncLifetime
{
    private readonly IContainer _container;

    public MinioFixture()
    {
        var configuredPath = Environment.GetEnvironmentVariable("MINIO_LICENSE_PATH");
        var solutionDirectory = CommonDirectoryPath.GetSolutionDirectory().DirectoryPath;
        var repositoryRoot = Directory.GetParent(solutionDirectory)!.Parent!.FullName;
        var licensePath = Path.GetFullPath(
            string.IsNullOrWhiteSpace(configuredPath) ? ".secrets/minio.license" : configuredPath,
            repositoryRoot);
        if (!File.Exists(licensePath))
        {
            throw new InvalidOperationException(
                "S3 integration tests require an AIStor license file. Set MINIO_LICENSE_PATH " +
                "or place the license at .secrets/minio.license in the repository root. " +
                "For a run without S3 integration tests, explicitly use --filter \"Category!=S3\".");
        }

        AccessKey = "fs-test-" + Guid.NewGuid().ToString("N")[..12];
        SecretKey = Guid.NewGuid().ToString("N");
        _container = new ContainerBuilder("quay.io/minio/aistor/minio:latest")
            .WithEnvironment("MINIO_ROOT_USER", AccessKey)
            .WithEnvironment("MINIO_ROOT_PASSWORD", SecretKey)
            .WithBindMount(licensePath, "/minio.license", AccessMode.ReadOnly)
            .WithPortBinding(9000, true)
            .WithCommand("minio", "server", "/data", "--console-address", ":9001", "--license", "/minio.license")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(
                request => request.ForPort(9000).ForPath("/minio/health/ready")))
            .Build();
    }

    public string AccessKey { get; }

    public string SecretKey { get; }

    public string Endpoint => $"http://{_container.Hostname}:{_container.GetMappedPublicPort(9000)}";

    public string PublicEndpoint => new Uri(Endpoint).Host == "127.0.0.1"
        ? $"http://localhost:{_container.GetMappedPublicPort(9000)}"
        : Endpoint;

    public static IConfiguration CreateConfiguration(S3Options options) =>
        new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["S3:Endpoint"] = options.Endpoint,
            ["S3:PublicEndpoint"] = options.PublicEndpoint,
            ["S3:Region"] = options.Region,
            ["S3:BucketName"] = options.BucketName,
            ["S3:AccessKey"] = options.AccessKey,
            ["S3:SecretKey"] = options.SecretKey,
            ["S3:ForcePathStyle"] = options.ForcePathStyle.ToString(),
            ["S3:CreateBucketIfMissing"] = options.CreateBucketIfMissing.ToString(),
            ["S3:PresignedUrlTtl"] = options.PresignedUrlTtl.ToString("c"),
            ["S3:RequestTimeout"] = options.RequestTimeout.ToString("c"),
            ["S3:HealthCheckTimeout"] = options.HealthCheckTimeout.ToString("c"),
        }).Build();

    public S3Options CreateOptions() =>
        new()
        {
            Endpoint = Endpoint,
            PublicEndpoint = PublicEndpoint,
            Region = "us-east-1",
            BucketName = "fs2-" + Guid.NewGuid().ToString("N"),
            AccessKey = AccessKey,
            SecretKey = SecretKey,
            ForcePathStyle = true,
            CreateBucketIfMissing = true,
        };

    public ServiceProvider CreateServices(S3Options options) =>
        new ServiceCollection()
            .AddLogging(builder => builder.SetMinimumLevel(LogLevel.Critical))
            .AddS3Storage(CreateConfiguration(options))
            .BuildServiceProvider();

    public async Task InitializeAsync() => await _container.StartAsync();

    public async Task DisposeAsync() => await _container.DisposeAsync();

    public Task PauseAsync() => _container.PauseAsync();

    public Task ResumeAsync() => _container.UnpauseAsync();
}