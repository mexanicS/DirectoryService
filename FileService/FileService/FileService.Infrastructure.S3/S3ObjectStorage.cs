using System.Collections.ObjectModel;
using System.Net;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using CSharpFunctionalExtensions;
using FileService.Core.ObjectStorage;
using FileService.Domain.MediaAssets;
using FileService.Domain.StorageReferences;
using FileService.Infrastructure.S3.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel;

namespace FileService.Infrastructure.S3;

public class S3ObjectStorage(
    [FromKeyedServices(Inject.OPERATIONS_CLIENT)] IAmazonS3 client,
    [FromKeyedServices(Inject.PRESIGNING_CLIENT)] IAmazonS3 presigner,
    IOptions<S3Options> options,
    TimeProvider timeProvider,
    ILogger<S3ObjectStorage> logger) : IObjectStorage
{
    private readonly S3Options _options = options.Value;

    public async Task<UnitResult<Error>> EnsureBucketAsync(CancellationToken cancellationToken)
    {
        var result = await ExecuteAsync(
            async token =>
            {
                try
                {
                    await ProbeBucketAsync(token);
                }
                catch (AmazonS3Exception exception) when (IsMissingBucket(exception) && _options.CreateBucketIfMissing)
                {
                    try
                    {
                        await client.PutBucketAsync(
                            new PutBucketRequest
                            {
                                BucketName = _options.BucketName,
                                BucketRegionName = _options.Region == "us-east-1" ? null : _options.Region,
                            },
                            token);
                    }
                    catch (AmazonS3Exception creationException) when (
                        creationException.ErrorCode is "BucketAlreadyOwnedByYou" or "BucketAlreadyExists")
                    {
                        // Another instance may have created it. Verify actual access below.
                    }

                    await ProbeBucketAsync(token);
                }

                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success<Error>() : result.Error;
    }

    public Task<Result<PresignedRequest, Error>> PresignPutAsync(
        StorageKey key,
        MimeType contentType,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (contentType is null)
        {
            return Task.FromResult<Result<PresignedRequest, Error>>(GeneralErrors.ValueIsRequired(nameof(MimeType)));
        }

        return PresignAsync(key, contentType, HttpVerb.PUT, cancellationToken);
    }

    public Task<Result<PresignedRequest, Error>> PresignGetAsync(
        StorageKey key,
        CancellationToken cancellationToken) =>
        PresignAsync(key, null, HttpVerb.GET, cancellationToken);

    public Task<Result<ObjectMetadata, Error>> GetMetadataAsync(
        StorageKey key,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (key is null)
        {
            return Task.FromResult<Result<ObjectMetadata, Error>>(GeneralErrors.ValueIsRequired(nameof(StorageKey)));
        }

        return ExecuteAsync(
            async token =>
            {
                var metadata = await client.GetObjectMetadataAsync(
                    new GetObjectMetadataRequest
                    {
                        BucketName = _options.BucketName,
                        Key = key.Value,
                    },
                    token);

                return new ObjectMetadata(metadata.ContentLength, metadata.Headers.ContentType ?? string.Empty, metadata.ETag);
            },
            cancellationToken,
            key);
    }

    public async Task<UnitResult<Error>> DeleteAsync(
        StorageKey key,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (key is null)
        {
            return GeneralErrors.ValueIsRequired(nameof(StorageKey));
        }

        var result = await ExecuteAsync(
            async token =>
            {
                try
                {
                    await client.DeleteObjectAsync(
                        new DeleteObjectRequest
                        {
                            BucketName = _options.BucketName,
                            Key = key.Value,
                        },
                        token);
                }
                catch (AmazonS3Exception exception) when (IsMissingObject(exception))
                {
                    // Deleting an already absent key is a successful, idempotent operation.
                }

                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success<Error>() : result.Error;
    }

    internal async Task<UnitResult<Error>> CheckBucketAsync(CancellationToken cancellationToken)
    {
        var result = await ExecuteAsync(
            async token =>
            {
                await ProbeBucketAsync(token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success<Error>() : result.Error;
    }

    private static bool IsMissingBucket(AmazonS3Exception exception) =>
        exception.ErrorCode == "NoSuchBucket" || exception.StatusCode == HttpStatusCode.NotFound;

    private static bool IsMissingObject(AmazonS3Exception exception) =>
        exception.ErrorCode != "NoSuchBucket" &&
        (exception.ErrorCode is "NoSuchKey" or "NotFound" || exception.StatusCode == HttpStatusCode.NotFound);

    private async Task<Result<PresignedRequest, Error>> PresignAsync(
        StorageKey key,
        MimeType? contentType,
        HttpVerb verb,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (key is null)
        {
            return GeneralErrors.ValueIsRequired(nameof(StorageKey));
        }

        return await ExecuteAsync(
            async token =>
            {
                var expiresAt = timeProvider.GetUtcNow().UtcDateTime.Add(_options.PresignedUrlTtl);
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = _options.BucketName,
                    Key = key.Value,
                    Verb = verb,
                    Expires = expiresAt,
                    Protocol = new Uri(_options.PublicEndpoint).Scheme == Uri.UriSchemeHttp ? Protocol.HTTP : Protocol.HTTPS,
                };

                var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                if (contentType is not null)
                {
                    request.ContentType = contentType.Value;
                    headers.Add("Content-Type", contentType.Value);
                }

                var url = await presigner.GetPreSignedURLAsync(request).WaitAsync(token);
                return new PresignedRequest(new Uri(url), expiresAt, new ReadOnlyDictionary<string, string>(headers));
            },
            cancellationToken);
    }

    private async Task ProbeBucketAsync(CancellationToken cancellationToken)
    {
        await client.ListObjectsV2Async(
            new ListObjectsV2Request
            {
                BucketName = _options.BucketName,
                MaxKeys = 1,
            },
            cancellationToken);
    }

    private async Task<Result<T, Error>> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken,
        StorageKey? key = null)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(_options.RequestTimeout);

        try
        {
            return await operation(timeout.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return StorageErrors.Unavailable();
        }
        catch (AmazonS3Exception exception)
        {
            logger.LogWarning(
                "S3 operation failed with status {StatusCode} and code {ErrorCode}",
                (int)exception.StatusCode,
                exception.ErrorCode);

            if (exception.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized)
            {
                return StorageErrors.AccessDenied();
            }

            return key is not null && IsMissingObject(exception)
                ? StorageErrors.ObjectNotFound(key)
                : StorageErrors.Unavailable();
        }
        catch (Exception exception) when (
            exception is AmazonClientException or HttpRequestException or IOException or TimeoutException)
        {
            // Exception messages can contain signed URLs. Log the type, not the credentials or URL.
            logger.LogWarning("S3 client operation failed ({ExceptionType})", exception.GetType().Name);
            return StorageErrors.Unavailable();
        }
    }

}