namespace FileService.Core.ObjectStorage;

public sealed record PresignedRequest(
    Uri Url,
    DateTime ExpiresAtUtc,
    IReadOnlyDictionary<string, string> RequiredHeaders);