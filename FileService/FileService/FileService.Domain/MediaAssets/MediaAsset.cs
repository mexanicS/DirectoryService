using CSharpFunctionalExtensions;
using FileService.Domain.StorageReferences;
using SharedKernel;

namespace FileService.Domain.MediaAssets;

public sealed class MediaAsset
{
    // EF Core
    private MediaAsset()
    {
    }

    private MediaAsset(
        MediaKind kind,
        UsageType usageType,
        FileName originalFileName,
        MimeType contentType,
        FileSize size,
        UploaderId uploadedBy,
        DraftId? draftId,
        EntityReference? entityReference,
        DateTime createdAt)
    {
        Id = MediaAssetId.New();
        Kind = kind;
        UsageType = usageType;
        OriginalFileName = originalFileName;
        ContentType = contentType;
        Size = size;
        UploadedBy = uploadedBy;
        DraftId = draftId;
        EntityReference = entityReference;
        Status = AssetStatus.AWAITING_UPLOAD;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
        Version = 1;
    }

    public MediaAssetId Id { get; private set; } = null!;

    public MediaKind Kind { get; private set; }

    public UsageType UsageType { get; private set; }

    public FileName OriginalFileName { get; private set; } = null!;

    public MimeType ContentType { get; private set; } = null!;

    public FileSize Size { get; private set; } = null!;

    public AssetStatus Status { get; private set; }

    public UploaderId UploadedBy { get; private set; } = null!;

    public DraftId? DraftId { get; private set; }

    public EntityReference? EntityReference { get; private set; }

    public bool IsTemporary => EntityReference is null;

    public StorageReference? StorageReference { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public FailureInfo? Failure { get; private set; }

    public DateTime? FailedAt { get; private set; }

    public DateTime? SoftDeletedAt { get; private set; }

    public bool IsDeleted => Status == AssetStatus.DELETED;

    public long Version { get; private set; }

    public static Result<MediaAsset, Error> CreateForUpload(
        MediaKind kind,
        UsageType usageType,
        FileName? originalFileName,
        MimeType? contentType,
        FileSize? size,
        UploaderId? uploadedBy,
        DraftId? draftId = null,
        EntityReference? entityReference = null,
        FilePolicy? policy = null,
        DateTime? now = null)
    {
        if (originalFileName is null)
        {
            return FileErrors.Required(nameof(FileName));
        }

        if (contentType is null)
        {
            return FileErrors.Required(nameof(MimeType));
        }

        if (size is null)
        {
            return FileErrors.Required(nameof(FileSize));
        }

        if (uploadedBy is null)
        {
            return FileErrors.Required(nameof(UploaderId));
        }

        if ((draftId is null) == (entityReference is null))
        {
            return FileErrors.InvalidBinding();
        }

        var validation = (policy ?? FilePolicy.Default).Validate(kind, usageType, contentType, size);
        if (validation.IsFailure)
        {
            return validation.Error;
        }

        var createdAt = now ?? DateTime.UtcNow;
        if (createdAt.Kind != DateTimeKind.Utc)
        {
            return FileErrors.InvalidTimestamp();
        }

        return new MediaAsset(kind, usageType, originalFileName, contentType, size,
            uploadedBy, draftId, entityReference, createdAt);
    }

    public UnitResult<Error> BeginUpload(StorageKey? key, DateTime? now = null)
    {
        if (Status != AssetStatus.AWAITING_UPLOAD)
        {
            return FileErrors.InvalidTransition(Status, nameof(BeginUpload));
        }

        if (key is null)
        {
            return FileErrors.Required(nameof(key));
        }

        var changedAt = now ?? DateTime.UtcNow;
        var timeValidation = ValidateTime(changedAt);
        if (timeValidation.IsFailure)
        {
            return timeValidation.Error;
        }

        StorageReference = FileService.Domain.StorageReferences.StorageReference.Reserve(Id, key, changedAt);
        Status = AssetStatus.UPLOADING;
        Touch(changedAt);
        return Result.Success<Error>();
    }

    public UnitResult<Error> ConfirmUpload(
        StorageKey? key,
        FileSize? actualSize,
        MimeType? detectedContentType,
        Sha256Checksum? checksum = null,
        ETag? etag = null,
        FilePolicy? policy = null,
        DateTime? now = null)
    {
        if (Status != AssetStatus.UPLOADING)
        {
            return FileErrors.InvalidTransition(Status, nameof(ConfirmUpload));
        }

        if (key is null || actualSize is null || detectedContentType is null)
        {
            return FileErrors.Required("upload metadata");
        }

        if (StorageReference is null || StorageReference.Key != key)
        {
            return FileErrors.StorageKeyMismatch();
        }

        if (actualSize != Size)
        {
            return FileErrors.SizeMismatch();
        }

        if (detectedContentType != ContentType)
        {
            return FileErrors.MimeMismatch();
        }

        var policyValidation = (policy ?? FilePolicy.Default).Validate(Kind, UsageType, detectedContentType, actualSize);
        if (policyValidation.IsFailure)
        {
            return policyValidation.Error;
        }

        var changedAt = now ?? DateTime.UtcNow;
        var timeValidation = ValidateTime(changedAt);
        if (timeValidation.IsFailure)
        {
            return timeValidation.Error;
        }

        StorageReference.Confirm(actualSize, detectedContentType, checksum, etag, changedAt);

        Status = AssetStatus.UPLOADED;
        Touch(changedAt);
        return Result.Success<Error>();
    }

    public UnitResult<Error> MarkReady(DateTime? now = null)
    {
        if (Status != AssetStatus.UPLOADED)
        {
            return FileErrors.InvalidTransition(Status, nameof(MarkReady));
        }

        if (StorageReference?.IsConfirmed != true)
        {
            return FileErrors.StorageNotConfirmed();
        }

        var changedAt = now ?? DateTime.UtcNow;
        var timeValidation = ValidateTime(changedAt);
        if (timeValidation.IsFailure)
        {
            return timeValidation.Error;
        }

        Status = AssetStatus.READY;
        Touch(changedAt);
        return Result.Success<Error>();
    }

    public UnitResult<Error> MarkFailed(FailureInfo? failure, DateTime? now = null)
    {
        if (Status is not (AssetStatus.UPLOADING or AssetStatus.UPLOADED))
        {
            return FileErrors.InvalidTransition(Status, nameof(MarkFailed));
        }

        if (failure is null)
        {
            return FileErrors.Required(nameof(failure));
        }

        var changedAt = now ?? DateTime.UtcNow;
        var timeValidation = ValidateTime(changedAt);
        if (timeValidation.IsFailure)
        {
            return timeValidation.Error;
        }

        Failure = failure;
        FailedAt = changedAt;
        Status = AssetStatus.FAILED;
        Touch(changedAt);
        return Result.Success<Error>();
    }

    public UnitResult<Error> AttachToEntity(EntityReference? reference, DateTime? now = null)
    {
        if (Status is AssetStatus.FAILED or AssetStatus.DELETED)
        {
            return FileErrors.InvalidTransition(Status, nameof(AttachToEntity));
        }

        if (reference is null)
        {
            return FileErrors.Required(nameof(reference));
        }

        if (EntityReference is not null)
        {
            return EntityReference == reference ? Result.Success<Error>() : FileErrors.AlreadyAttached();
        }

        var changedAt = now ?? DateTime.UtcNow;
        var timeValidation = ValidateTime(changedAt);
        if (timeValidation.IsFailure)
        {
            return timeValidation.Error;
        }

        EntityReference = reference;
        DraftId = null;
        Touch(changedAt);
        return Result.Success<Error>();
    }

    public UnitResult<Error> Delete(DateTime? now = null)
    {
        if (Status == AssetStatus.DELETED)
        {
            return Result.Success<Error>();
        }

        var changedAt = now ?? DateTime.UtcNow;
        var timeValidation = ValidateTime(changedAt);
        if (timeValidation.IsFailure)
        {
            return timeValidation.Error;
        }

        SoftDeletedAt = changedAt;
        Status = AssetStatus.DELETED;
        Touch(changedAt);
        return Result.Success<Error>();
    }

    private UnitResult<Error> ValidateTime(DateTime now)
    {
        if (now.Kind != DateTimeKind.Utc || now < UpdatedAt)
        {
            return FileErrors.InvalidTimestamp();
        }

        return Result.Success<Error>();
    }

    private void Touch(DateTime now)
    {
        UpdatedAt = now;
        Version++;
    }
}