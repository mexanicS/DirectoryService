using FileService.Domain.MediaAssets;

namespace FileService.Domain.StorageReferences;

public sealed class StorageReference
{
    // EF Core
    private StorageReference()
    {
    }

    private StorageReference(MediaAssetId mediaAssetId, StorageKey key, DateTime reservedAt)
    {
        MediaAssetId = mediaAssetId;
        Key = key;
        ReservedAt = reservedAt;
    }

    public MediaAssetId MediaAssetId { get; private set; } = null!;

    public StorageKey Key { get; private set; } = null!;

    public DateTime ReservedAt { get; private set; }

    public FileSize? ActualSize { get; private set; }

    public MimeType? DetectedContentType { get; private set; }

    public Sha256Checksum? Checksum { get; private set; }

    public ETag? ETag { get; private set; }

    public DateTime? ConfirmedAt { get; private set; }

    public bool IsConfirmed => ConfirmedAt is not null;

    internal static StorageReference Reserve(MediaAssetId mediaAssetId, StorageKey key, DateTime reservedAt) =>
        new(mediaAssetId, key, reservedAt);

    internal void Confirm(
        FileSize actualSize,
        MimeType detectedContentType,
        Sha256Checksum? checksum,
        ETag? etag,
        DateTime confirmedAt)
    {
        ActualSize = actualSize;
        DetectedContentType = detectedContentType;
        Checksum = checksum;
        ETag = etag;
        ConfirmedAt = confirmedAt;
    }
}