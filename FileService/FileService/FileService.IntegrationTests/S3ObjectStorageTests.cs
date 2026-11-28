using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using FileService.Core.ObjectStorage;
using FileService.Domain.MediaAssets;
using FileService.Domain.StorageReferences;
using Microsoft.Extensions.DependencyInjection;

namespace FileService.IntegrationTests;

[Trait("Category", "S3")]
public class S3ObjectStorageTests(MinioFixture fixture) : IClassFixture<MinioFixture>
{
    [Fact]
    public async Task StorageFlow_UsesPresignedUrlsReadsMetadataAndDeletesIdempotently()
    {
        var options = fixture.CreateOptions();
        using var services = fixture.CreateServices(options);
        var storage = services.GetRequiredService<IObjectStorage>();
        Assert.True((await storage.EnsureBucketAsync(default)).IsSuccess);
        Assert.True((await storage.EnsureBucketAsync(default)).IsSuccess);
        var key = StorageKeyGenerator.CreateOriginal(MediaAssetId.New()).Value;
        var mime = MimeType.Create("text/plain").Value;
        var bytes = Encoding.UTF8.GetBytes("FileService S3 integration test");
        using var http = new HttpClient();

        var upload = await storage.PresignPutAsync(key, mime, default);
        Assert.True(upload.IsSuccess);
        Assert.Equal(new Uri(options.PublicEndpoint).Host, upload.Value.Url.Host);
        Assert.Equal(new Uri(options.PublicEndpoint).Port, upload.Value.Url.Port);
        Assert.Equal("text/plain", upload.Value.RequiredHeaders["Content-Type"]);
        var expiresPart = upload.Value.Url.Query.TrimStart('?').Split('&')
            .Single(part => part.StartsWith("X-Amz-Expires=", StringComparison.Ordinal));
        var expiresSeconds = int.Parse(expiresPart.Split('=')[1], CultureInfo.InvariantCulture);
        Assert.InRange(expiresSeconds, 898, 900);
        Assert.Equal(DateTimeKind.Utc, upload.Value.ExpiresAtUtc.Kind);
        using var content = new ByteArrayContent(bytes);
        foreach (var header in upload.Value.RequiredHeaders)
        {
            content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        using var uploadResponse = await http.PutAsync(upload.Value.Url, content);
        uploadResponse.EnsureSuccessStatusCode();
        var metadata = await storage.GetMetadataAsync(key, default);
        Assert.True(metadata.IsSuccess);
        Assert.Equal(bytes.LongLength, metadata.Value.Size);
        Assert.Equal("text/plain", metadata.Value.ContentType);
        Assert.False(string.IsNullOrWhiteSpace(metadata.Value.ETag));

        var download = await storage.PresignGetAsync(key, default);
        Assert.True(download.IsSuccess);
        Assert.Equal(upload.Value.Url.Host, download.Value.Url.Host);
        Assert.Equal(bytes, await http.GetByteArrayAsync(download.Value.Url));
        Assert.True((await storage.DeleteAsync(key, default)).IsSuccess);
        Assert.True((await storage.DeleteAsync(key, default)).IsSuccess);
        var missing = await storage.GetMetadataAsync(key, default);
        Assert.True(missing.IsFailure);
        Assert.Equal("storage.object.not_found", missing.Error.Code);
    }

    [Theory]
    [InlineData("application/pdf", HttpStatusCode.Forbidden)]
    [InlineData(null, HttpStatusCode.BadRequest)]
    public async Task PresignedPut_RejectsMissingOrDifferentContentType(string? contentType, HttpStatusCode expectedStatus)
    {
        var options = fixture.CreateOptions();
        using var services = fixture.CreateServices(options);
        var storage = services.GetRequiredService<IObjectStorage>();
        Assert.True((await storage.EnsureBucketAsync(default)).IsSuccess);
        var key = StorageKeyGenerator.CreateOriginal(MediaAssetId.New()).Value;
        var upload = await storage.PresignPutAsync(key, MimeType.Create("text/plain").Value, default);
        Assert.True(upload.IsSuccess);
        using var http = new HttpClient();
        using var content = new ByteArrayContent([1, 2, 3]);
        if (contentType is not null)
        {
            content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        }

        using var response = await http.PutAsync(upload.Value.Url, content);

        Assert.Equal(expectedStatus, response.StatusCode);
        var metadata = await storage.GetMetadataAsync(key, default);
        Assert.True(metadata.IsFailure);
        Assert.Equal("storage.object.not_found", metadata.Error.Code);
    }

    [Fact]
    public async Task PresignedPut_RejectsChangedStorageKey()
    {
        using var services = fixture.CreateServices(fixture.CreateOptions());
        var storage = services.GetRequiredService<IObjectStorage>();
        Assert.True((await storage.EnsureBucketAsync(default)).IsSuccess);
        var key = StorageKeyGenerator.CreateOriginal(MediaAssetId.New()).Value;
        var upload = await storage.PresignPutAsync(key, MimeType.Create("text/plain").Value, default);
        Assert.True(upload.IsSuccess);
        var changedUrl = new UriBuilder(upload.Value.Url)
        {
            Path = upload.Value.Url.AbsolutePath + "-another-object",
        };
        using var http = new HttpClient();
        using var content = new ByteArrayContent([1, 2, 3]);
        content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");

        using var response = await http.PutAsync(changedUrl.Uri, content);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task PresignedUrls_RejectRequestsAfterExpiration()
    {
        var options = fixture.CreateOptions();
        options.PresignedUrlTtl = TimeSpan.FromSeconds(5);
        using var services = fixture.CreateServices(options);
        var storage = services.GetRequiredService<IObjectStorage>();
        Assert.True((await storage.EnsureBucketAsync(default)).IsSuccess);
        var key = StorageKeyGenerator.CreateOriginal(MediaAssetId.New()).Value;
        var mime = MimeType.Create("text/plain").Value;
        var bytes = Encoding.UTF8.GetBytes("A short-lived signed URL");
        var upload = await storage.PresignPutAsync(key, mime, default);
        Assert.True(upload.IsSuccess);
        using var http = new HttpClient();
        using var content = new ByteArrayContent(bytes);
        content.Headers.ContentType = new MediaTypeHeaderValue(mime.Value);
        using var uploaded = await http.PutAsync(upload.Value.Url, content);
        uploaded.EnsureSuccessStatusCode();
        var download = await storage.PresignGetAsync(key, default);
        Assert.True(download.IsSuccess);
        Assert.Equal(bytes, await http.GetByteArrayAsync(download.Value.Url));

        // Wait beyond the real signature lifetime, including its second-level precision.
        var delay = download.Value.ExpiresAtUtc - DateTime.UtcNow + TimeSpan.FromSeconds(2);
        if (delay > TimeSpan.Zero)
        {
            await Task.Delay(delay);
        }

        using var expiredContent = new ByteArrayContent([1, 2, 3]);
        expiredContent.Headers.ContentType = new MediaTypeHeaderValue(mime.Value);
        using var expiredUpload = await http.PutAsync(upload.Value.Url, expiredContent);
        using var expiredDownload = await http.GetAsync(download.Value.Url);
        Assert.Equal(HttpStatusCode.Forbidden, expiredUpload.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, expiredDownload.StatusCode);
        var metadata = await storage.GetMetadataAsync(key, default);
        Assert.True(metadata.IsSuccess);
        Assert.Equal(bytes.LongLength, metadata.Value.Size);
    }

    [Fact]
    public async Task Delete_AlreadyMissingObject_IsSuccessful()
    {
        using var services = fixture.CreateServices(fixture.CreateOptions());
        var storage = services.GetRequiredService<IObjectStorage>();
        Assert.True((await storage.EnsureBucketAsync(default)).IsSuccess);

        var result = await storage.DeleteAsync(StorageKey.Create("originals/missing-object").Value, default);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task EnsureBucket_DoesNotCreateBucketWhenDisabled()
    {
        var options = fixture.CreateOptions();
        options.CreateBucketIfMissing = false;
        using var services = fixture.CreateServices(options);
        var storage = services.GetRequiredService<IObjectStorage>();

        var result = await storage.EnsureBucketAsync(default);

        Assert.True(result.IsFailure);
        Assert.Equal("storage.unavailable", result.Error.Code);
    }

    [Fact]
    public async Task EnsureBucket_InvalidCredentials_ReturnAccessDenied()
    {
        var options = fixture.CreateOptions();
        options.SecretKey = "incorrect-secret";
        using var services = fixture.CreateServices(options);
        var storage = services.GetRequiredService<IObjectStorage>();

        var result = await storage.EnsureBucketAsync(default);

        Assert.True(result.IsFailure);
        Assert.Equal("storage.access_denied", result.Error.Code);
    }

    [Fact]
    public async Task EnsureBucket_ConcurrentInitializations_AreSafe()
    {
        var options = fixture.CreateOptions();
        using var firstServices = fixture.CreateServices(options);
        using var secondServices = fixture.CreateServices(options);
        var first = firstServices.GetRequiredService<IObjectStorage>();
        var second = secondServices.GetRequiredService<IObjectStorage>();

        var results = await Task.WhenAll(first.EnsureBucketAsync(default), second.EnsureBucketAsync(default));

        Assert.All(results, result => Assert.True(result.IsSuccess));
    }

    [Fact]
    public async Task HealthCheck_DoesNotCreateMissingBucket()
    {
        using var services = fixture.CreateServices(fixture.CreateOptions());
        var healthChecks = services.GetRequiredService<Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckService>();

        var report = await healthChecks.CheckHealthAsync();

        Assert.Equal(Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy, report.Status);
    }

    [Fact]
    public async Task Delete_MissingBucket_ReturnsStorageError()
    {
        using var services = fixture.CreateServices(fixture.CreateOptions());
        var storage = services.GetRequiredService<IObjectStorage>();

        var result = await storage.DeleteAsync(StorageKey.Create("originals/missing").Value, default);

        Assert.True(result.IsFailure);
        Assert.Equal("storage.unavailable", result.Error.Code);
    }

    [Fact]
    public async Task StorageOperations_RespectCallerCancellation()
    {
        using var services = fixture.CreateServices(fixture.CreateOptions());
        var storage = services.GetRequiredService<IObjectStorage>();
        var token = new CancellationToken(true);
        var key = StorageKey.Create("originals/cancelled").Value;

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => storage.EnsureBucketAsync(token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => storage.PresignPutAsync(key, MimeType.Create("text/plain").Value, token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => storage.PresignGetAsync(key, token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => storage.GetMetadataAsync(key, token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => storage.DeleteAsync(key, token));
    }
}