using CSharpFunctionalExtensions;
using FileService.Domain.MediaAssets;
using FileService.Domain.StorageReferences;
using SharedKernel;

namespace FileService.Core.ObjectStorage;

public interface IObjectStorage
{
    /// <summary>
    /// Ensures that the configured bucket exists and is accessible.
    /// Repeated calls must be safe.
    /// </summary>
    Task<UnitResult<Error>> EnsureBucketAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Creates a temporary PUT URL and the headers required to use it.
    /// </summary>
    Task<Result<PresignedRequest, Error>> PresignPutAsync(
        StorageKey key,
        MimeType contentType,
        CancellationToken cancellationToken);

    /// <summary>
    /// Creates a temporary GET URL for the specified object.
    /// </summary>
    Task<Result<PresignedRequest, Error>> PresignGetAsync(
        StorageKey key,
        CancellationToken cancellationToken);

    /// <summary>
    /// Reads metadata from storage. The stored content type is not a content inspection result.
    /// </summary>
    Task<Result<ObjectMetadata, Error>> GetMetadataAsync(
        StorageKey key,
        CancellationToken cancellationToken);

    /// <summary>
    /// Deletes the object. An already absent object is considered successfully deleted.
    /// </summary>
    Task<UnitResult<Error>> DeleteAsync(
        StorageKey key,
        CancellationToken cancellationToken);
}