namespace FileService.Core.ObjectStorage;

public sealed record ObjectMetadata(
    long Size,
    string ContentType,
    string? ETag = null);