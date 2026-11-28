using CSharpFunctionalExtensions;
using FileService.Core.ObjectStorage;
using FileService.Domain.MediaAssets;
using FileService.Domain.StorageReferences;
using FileService.Infrastructure.S3;
using Microsoft.Extensions.Logging.Abstractions;
using SharedKernel;

namespace FileService.UnitTests.ObjectStorage;

public class S3BucketInitializerTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task Initialization_CompletesAfterFirstSuccess(int failuresBeforeSuccess)
    {
        var storage = new StubObjectStorage(failuresBeforeSuccess);
        using var initializer = new S3BucketInitializer(storage, NullLogger<S3BucketInitializer>.Instance);
        await initializer.StartAsync(default);

        try
        {
            var execution = Assert.IsAssignableFrom<Task>(initializer.ExecuteTask);
            await execution.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.Equal(failuresBeforeSuccess + 1, storage.EnsureCalls);
        }
        finally
        {
            await initializer.StopAsync(default);
        }
    }

    private sealed class StubObjectStorage(int failuresBeforeSuccess) : IObjectStorage
    {
        public int EnsureCalls { get; private set; }

        public Task<UnitResult<Error>> EnsureBucketAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            EnsureCalls++;
            return Task.FromResult<UnitResult<Error>>(
                EnsureCalls <= failuresBeforeSuccess ? StorageErrors.Unavailable() : Result.Success<Error>());
        }

        public Task<Result<PresignedRequest, Error>> PresignPutAsync(
            StorageKey key,
            MimeType contentType,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<Result<PresignedRequest, Error>> PresignGetAsync(
            StorageKey key,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<Result<ObjectMetadata, Error>> GetMetadataAsync(
            StorageKey key,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<UnitResult<Error>> DeleteAsync(
            StorageKey key,
            CancellationToken cancellationToken) => throw new NotSupportedException();
    }
}