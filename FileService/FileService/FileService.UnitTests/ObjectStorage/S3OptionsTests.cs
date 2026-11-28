using FileService.Infrastructure.S3.Options;

namespace FileService.UnitTests.ObjectStorage;

public class S3OptionsTests
{
    [Fact]
    public void Validate_AcceptsConfiguredStorage()
    {
        Assert.True(new S3OptionsValidator().Validate(null, ValidOptions()).Succeeded);
    }

    [Theory]
    [InlineData("")]
    [InlineData("localhost:9000")]
    [InlineData("ftp://localhost:9000")]
    [InlineData("http://user:password@localhost:9000")]
    [InlineData("http://localhost:9000?signature=secret")]
    [InlineData("http://localhost:9000#fragment")]
    public void Validate_RejectsInvalidOrCredentialBearingEndpoints(string endpoint)
    {
        var options = ValidOptions();
        options.Endpoint = endpoint;
        Assert.True(new S3OptionsValidator().Validate(null, options).Failed);

        options = ValidOptions();
        options.PublicEndpoint = endpoint;
        Assert.True(new S3OptionsValidator().Validate(null, options).Failed);
    }

    [Theory]
    [InlineData("")]
    [InlineData("ab")]
    [InlineData("Files")]
    [InlineData("../files")]
    [InlineData("files..test")]
    [InlineData("files.-test")]
    [InlineData("192.168.1.1")]
    public void Validate_RejectsInvalidBucketNames(string bucketName)
    {
        var options = ValidOptions();
        options.BucketName = bucketName;

        Assert.True(new S3OptionsValidator().Validate(null, options).Failed);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(604801)]
    public void Validate_RejectsInvalidUrlLifetime(int seconds)
    {
        var options = ValidOptions();
        options.PresignedUrlTtl = TimeSpan.FromSeconds(seconds);

        Assert.True(new S3OptionsValidator().Validate(null, options).Failed);
    }

    [Fact]
    public void Validate_RejectsMissingCredentialsAndInvalidTimeoutsWithoutExposingSecrets()
    {
        var options = ValidOptions();
        options.AccessKey = string.Empty;
        options.RequestTimeout = TimeSpan.Zero;
        options.HealthCheckTimeout = TimeSpan.FromSeconds(-1);
        options.SecretKey = "secret-value\n";

        var result = new S3OptionsValidator().Validate(null, options);

        Assert.True(result.Failed);
        Assert.DoesNotContain("secret-value", result.FailureMessage, StringComparison.Ordinal);
    }

    private static S3Options ValidOptions() =>
        new()
        {
            Endpoint = "http://127.0.0.1:9000",
            PublicEndpoint = "http://localhost:9000",
            AccessKey = "test-access",
            SecretKey = "test-secret",
            ForcePathStyle = true,
        };
}