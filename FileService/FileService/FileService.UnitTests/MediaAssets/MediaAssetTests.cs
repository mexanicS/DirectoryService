using System.Text.Json;
using CSharpFunctionalExtensions;
using FileService.Domain.MediaAssets;
using FileService.Domain.StorageReferences;
using SharedKernel;

namespace FileService.UnitTests.MediaAssets;

public class MediaAssetTests
{
    private static readonly DateTime CreatedAt = new(2026, 11, 13, 0, 0, 0, DateTimeKind.Utc);
    private static readonly StorageKey Key = StorageKey.Create("originals/asset/image.png").Value;
    private static readonly FileSize Size = FileSize.Create(100).Value;
    private static readonly MimeType Mime = MimeType.Create("image/png").Value;
    private static readonly FailureInfo Failure = FailureInfo.Create("upload.failed", "Не удалось загрузить файл.").Value;
    private static readonly EntityReference Reference = EntityReference.Create("user", Guid.NewGuid()).Value;

    public static IEnumerable<object[]> Transitions()
    {
        foreach (var status in Enum.GetValues<AssetStatus>())
        {
            yield return [status, "begin", status == AssetStatus.AWAITING_UPLOAD, AssetStatus.UPLOADING];
            yield return [status, "confirm", status == AssetStatus.UPLOADING, AssetStatus.UPLOADED];
            yield return [status, "ready", status == AssetStatus.UPLOADED, AssetStatus.READY];
            yield return [status, "fail", status is AssetStatus.UPLOADING or AssetStatus.UPLOADED, AssetStatus.FAILED];
            yield return [status, "attach", status is not (AssetStatus.FAILED or AssetStatus.DELETED), status];
            yield return [status, "delete", true, AssetStatus.DELETED];
        }
    }

    [Theory]
    [MemberData(nameof(Transitions))]
    public void Operations_FollowTransitionTable_AndFailuresAreAtomic(
        AssetStatus source, string operation, bool allowed, AssetStatus expected)
    {
        var asset = CreateInStatus(source);
        var before = Snapshot(asset);
        var previousVersion = asset.Version;
        var changedAt = CreatedAt.AddDays(1);

        var result = Apply(asset, operation, changedAt);

        if (!allowed)
        {
            Assert.True(result.IsFailure);
            Assert.Equal("file.invalid_transition", result.Error.Code);
            Assert.Equal(before, Snapshot(asset));
            return;
        }

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, asset.Status);
        if (source == AssetStatus.DELETED && operation == "delete")
        {
            Assert.Equal(before, Snapshot(asset));
        }
        else
        {
            Assert.Equal(previousVersion + 1, asset.Version);
            Assert.Equal(changedAt, asset.UpdatedAt);
        }
    }

    [Fact]
    public void CreateForUpload_InitializesTemporaryAsset()
    {
        var asset = CreateInStatus(AssetStatus.AWAITING_UPLOAD);

        Assert.Equal('7', asset.Id.Value.ToString("D")[14]);
        Assert.Equal(AssetStatus.AWAITING_UPLOAD, asset.Status);
        Assert.True(asset.IsTemporary);
        Assert.NotNull(asset.DraftId);
        Assert.Null(asset.EntityReference);
        Assert.Null(asset.StorageReference);
        Assert.Null(asset.Failure);
        Assert.Null(asset.FailedAt);
        Assert.Null(asset.SoftDeletedAt);
        Assert.False(asset.IsDeleted);
        Assert.Equal(CreatedAt, asset.CreatedAt);
        Assert.Equal(CreatedAt, asset.UpdatedAt);
        Assert.Equal(1, asset.Version);
    }

    [Fact]
    public void CreateForUpload_CanStartAttachedBeforeUpload()
    {
        var asset = Create(entityReference: Reference).Value;

        Assert.False(asset.IsTemporary);
        Assert.Null(asset.DraftId);
        Assert.Equal(Reference, asset.EntityReference);
        Assert.Equal(AssetStatus.AWAITING_UPLOAD, asset.Status);
    }

    [Fact]
    public void CreateForUpload_RejectsMissingAndAmbiguousBinding()
    {
        var name = FileName.Create("image.png").Value;
        var uploader = UploaderId.Create(Guid.NewGuid()).Value;

        var missing = MediaAsset.CreateForUpload(MediaKind.IMAGE, UsageType.AVATAR,
            name, Mime, Size, uploader);
        var ambiguous = MediaAsset.CreateForUpload(MediaKind.IMAGE, UsageType.AVATAR,
            name, Mime, Size, uploader, DraftId.Create(Guid.NewGuid()).Value, Reference);

        Assert.Equal("file.binding.invalid", missing.Error.Code);
        Assert.Equal("file.binding.invalid", ambiguous.Error.Code);
    }

    [Fact]
    public void CreateForUpload_RejectsMissingValuesInvalidTimeAndPolicy()
    {
        var name = FileName.Create("image.png").Value;
        var uploader = UploaderId.Create(Guid.NewGuid()).Value;
        var draft = DraftId.Create(Guid.NewGuid()).Value;

        Assert.True(MediaAsset.CreateForUpload(MediaKind.IMAGE, UsageType.AVATAR,
            null, Mime, Size, uploader, draft).IsFailure);
        Assert.True(MediaAsset.CreateForUpload(MediaKind.IMAGE, UsageType.AVATAR,
            name, null, Size, uploader, draft).IsFailure);
        Assert.True(MediaAsset.CreateForUpload(MediaKind.IMAGE, UsageType.AVATAR,
            name, Mime, null, uploader, draft).IsFailure);
        Assert.True(MediaAsset.CreateForUpload(MediaKind.IMAGE, UsageType.AVATAR,
            name, Mime, Size, null, draft).IsFailure);
        Assert.Equal("file.timestamp.invalid", Create(now: DateTime.SpecifyKind(CreatedAt, DateTimeKind.Unspecified)).Error.Code);
        Assert.Equal("file.size_exceeded", Create(policy: FilePolicy.Create(maxAvatarSize: 99).Value).Error.Code);
    }

    [Fact]
    public void Upload_ReservesKeyAndConfirmsSeparateMetadata()
    {
        var asset = CreateInStatus(AssetStatus.AWAITING_UPLOAD);
        Assert.True(asset.BeginUpload(Key, CreatedAt.AddSeconds(1)).IsSuccess);
        var reservation = Assert.IsType<StorageReference>(asset.StorageReference);

        Assert.False(reservation.IsConfirmed);
        Assert.Null(reservation.ActualSize);
        Assert.Null(reservation.DetectedContentType);
        Assert.Null(reservation.ETag);
        Assert.Null(reservation.Checksum);
        Assert.Equal(asset.Id, reservation.MediaAssetId);

        var checksum = Sha256Checksum.Create(new string('a', 64)).Value;
        var etag = ETag.Create("\"Opaque-ETag\"").Value;
        Assert.True(asset.ConfirmUpload(Key, Size, Mime, checksum, etag,
            now: CreatedAt.AddSeconds(2)).IsSuccess);

        var confirmed = Assert.IsType<StorageReference>(asset.StorageReference);
        Assert.True(confirmed.IsConfirmed);
        Assert.Equal(Size, confirmed.ActualSize);
        Assert.Equal(Mime, confirmed.DetectedContentType);
        Assert.Equal(checksum, confirmed.Checksum);
        Assert.Equal(etag, confirmed.ETag);
        Assert.Equal(reservation.ReservedAt, confirmed.ReservedAt);
        Assert.Equal(CreatedAt.AddSeconds(2), confirmed.ConfirmedAt);
        Assert.False(reservation.IsConfirmed);
        Assert.True(asset.MarkReady(CreatedAt.AddSeconds(3)).IsSuccess);
        Assert.True(asset.IsTemporary);
    }

    [Theory]
    [InlineData("key", "file.storage_key_mismatch")]
    [InlineData("size", "file.size_mismatch")]
    [InlineData("mime", "file.mime_mismatch")]
    [InlineData("policy", "file.size_exceeded")]
    [InlineData("time", "file.timestamp.invalid")]
    public void ConfirmUpload_RejectsInvalidConfirmationWithoutAnyChanges(string invalidPart, string expectedCode)
    {
        var asset = CreateInStatus(AssetStatus.UPLOADING);
        var before = Snapshot(asset);

        var result = asset.ConfirmUpload(
            invalidPart == "key" ? StorageKey.Create("originals/other/image.png").Value : Key,
            invalidPart == "size" ? FileSize.Create(99).Value : Size,
            invalidPart == "mime" ? MimeType.Create("image/jpeg").Value : Mime,
            checksum: Sha256Checksum.Create(new string('a', 64)).Value,
            etag: ETag.Create("tag").Value,
            policy: invalidPart == "policy" ? FilePolicy.Create(maxAvatarSize: 99).Value : null,
            now: invalidPart == "time" ? CreatedAt : CreatedAt.AddDays(1));

        Assert.True(result.IsFailure);
        Assert.Equal(expectedCode, result.Error.Code);
        Assert.Equal(before, Snapshot(asset));
    }

    [Fact]
    public void Operations_RejectNullArgumentsWithoutMutation()
    {
        var pending = CreateInStatus(AssetStatus.AWAITING_UPLOAD);
        var pendingBefore = Snapshot(pending);

        Assert.True(pending.BeginUpload(null).IsFailure);
        Assert.True(pending.AttachToEntity(null).IsFailure);
        Assert.Equal(pendingBefore, Snapshot(pending));

        var uploading = CreateInStatus(AssetStatus.UPLOADING);
        var uploadingBefore = Snapshot(uploading);

        Assert.True(uploading.ConfirmUpload(null, Size, Mime).IsFailure);
        Assert.True(uploading.ConfirmUpload(Key, null, Mime).IsFailure);
        Assert.True(uploading.ConfirmUpload(Key, Size, null).IsFailure);
        Assert.True(uploading.MarkFailed(null).IsFailure);
        Assert.Equal(uploadingBefore, Snapshot(uploading));
    }

    [Fact]
    public void AttachToEntity_ClearsDraft_AndDoesNotChangeUploadStateOrAuthor()
    {
        var asset = CreateInStatus(AssetStatus.UPLOADING);
        var author = asset.UploadedBy;
        var storage = asset.StorageReference;

        Assert.True(asset.AttachToEntity(Reference, CreatedAt.AddMinutes(1)).IsSuccess);

        Assert.False(asset.IsTemporary);
        Assert.Null(asset.DraftId);
        Assert.Equal(Reference, asset.EntityReference);
        Assert.Equal(author, asset.UploadedBy);
        Assert.Same(storage, asset.StorageReference);
        Assert.Equal(AssetStatus.UPLOADING, asset.Status);
    }

    [Fact]
    public void AttachToEntity_IsIdempotentForSameEntity_AndRejectsReassignment()
    {
        var asset = Create(entityReference: Reference).Value;
        var before = Snapshot(asset);
        var sameReference = EntityReference.Create("USER", Reference.Id).Value;

        Assert.True(asset.AttachToEntity(sameReference, CreatedAt.AddYears(-1)).IsSuccess);
        Assert.Equal(before, Snapshot(asset));

        var result = asset.AttachToEntity(EntityReference.Create("user", Guid.NewGuid()).Value);
        Assert.Equal("file.already_attached", result.Error.Code);
        Assert.Equal(before, Snapshot(asset));
    }

    [Theory]
    [InlineData(AssetStatus.FAILED)]
    [InlineData(AssetStatus.DELETED)]
    public void AttachToEntity_RejectsSameEntityInTerminalStates(AssetStatus status)
    {
        var asset = Create(entityReference: Reference).Value;
        Assert.True(asset.BeginUpload(Key, CreatedAt.AddSeconds(1)).IsSuccess);
        Assert.True(asset.MarkFailed(Failure, CreatedAt.AddSeconds(2)).IsSuccess);
        if (status == AssetStatus.DELETED)
        {
            Assert.True(asset.Delete(CreatedAt.AddSeconds(3)).IsSuccess);
        }

        var before = Snapshot(asset);

        Assert.True(asset.AttachToEntity(Reference).IsFailure);
        Assert.Equal(before, Snapshot(asset));
    }

    [Fact]
    public void Delete_PreservesFailureAndConfirmedStorage_AndIsIdempotent()
    {
        var asset = CreateInStatus(AssetStatus.UPLOADED);
        var storage = asset.StorageReference;
        Assert.True(asset.MarkFailed(Failure, CreatedAt.AddMinutes(1)).IsSuccess);

        Assert.True(asset.Delete(CreatedAt.AddMinutes(2)).IsSuccess);
        Assert.True(asset.IsDeleted);
        Assert.Equal(CreatedAt.AddMinutes(2), asset.SoftDeletedAt);
        Assert.Equal(CreatedAt.AddMinutes(1), asset.FailedAt);
        Assert.Equal(Failure, asset.Failure);
        Assert.Same(storage, asset.StorageReference);
        var before = Snapshot(asset);

        Assert.True(asset.Delete(CreatedAt.AddYears(-1)).IsSuccess);
        Assert.Equal(before, Snapshot(asset));
    }

    [Theory]
    [InlineData("begin", AssetStatus.AWAITING_UPLOAD)]
    [InlineData("confirm", AssetStatus.UPLOADING)]
    [InlineData("ready", AssetStatus.UPLOADED)]
    [InlineData("fail", AssetStatus.UPLOADING)]
    [InlineData("attach", AssetStatus.READY)]
    [InlineData("delete", AssetStatus.READY)]
    public void Operations_RejectBackdatedAndNonUtcTimeWithoutMutation(string operation, AssetStatus status)
    {
        var asset = CreateInStatus(status);
        var before = Snapshot(asset);

        var backdated = Apply(asset, operation, CreatedAt.AddDays(-1));
        var nonUtc = Apply(asset, operation, DateTime.SpecifyKind(CreatedAt.AddDays(1), DateTimeKind.Unspecified));

        Assert.Equal("file.timestamp.invalid", backdated.Error.Code);
        Assert.Equal("file.timestamp.invalid", nonUtc.Error.Code);
        Assert.Equal(before, Snapshot(asset));
    }

    [Fact]
    public void AggregateAndStorage_DoNotExposePublicMutation()
    {
        Assert.Empty(typeof(MediaAsset).GetConstructors());
        Assert.Empty(typeof(StorageReference).GetConstructors());
        Assert.All(typeof(MediaAsset).GetProperties(), property => Assert.False(property.SetMethod?.IsPublic == true));
        Assert.All(typeof(StorageReference).GetProperties(), property => Assert.False(property.SetMethod?.IsPublic == true));
    }

    private static UnitResult<Error> Apply(MediaAsset asset, string operation, DateTime now) =>
        operation switch
        {
            "begin" => asset.BeginUpload(Key, now),
            "confirm" => asset.ConfirmUpload(Key, Size, Mime, now: now),
            "ready" => asset.MarkReady(now),
            "fail" => asset.MarkFailed(Failure, now),
            "attach" => asset.AttachToEntity(Reference, now),
            "delete" => asset.Delete(now),
            _ => throw new ArgumentOutOfRangeException(nameof(operation)),
        };

    private static MediaAsset CreateInStatus(AssetStatus status)
    {
        var asset = Create().Value;
        if (status == AssetStatus.AWAITING_UPLOAD)
        {
            return asset;
        }

        Assert.True(asset.BeginUpload(Key, CreatedAt.AddSeconds(1)).IsSuccess);
        if (status == AssetStatus.UPLOADING)
        {
            return asset;
        }

        if (status == AssetStatus.FAILED)
        {
            Assert.True(asset.MarkFailed(Failure, CreatedAt.AddSeconds(2)).IsSuccess);
            return asset;
        }

        Assert.True(asset.ConfirmUpload(Key, Size, Mime, now: CreatedAt.AddSeconds(2)).IsSuccess);
        if (status == AssetStatus.UPLOADED)
        {
            return asset;
        }

        Assert.True(asset.MarkReady(CreatedAt.AddSeconds(3)).IsSuccess);
        if (status == AssetStatus.DELETED)
        {
            Assert.True(asset.Delete(CreatedAt.AddSeconds(4)).IsSuccess);
        }

        return asset;
    }

    private static Result<MediaAsset, Error> Create(
        EntityReference? entityReference = null, FilePolicy? policy = null, DateTime? now = null) =>
        MediaAsset.CreateForUpload(
            MediaKind.IMAGE,
            UsageType.AVATAR,
            FileName.Create("Аватар.PNG").Value,
            Mime,
            Size,
            UploaderId.Create(Guid.NewGuid()).Value,
            entityReference is null ? DraftId.Create(Guid.NewGuid()).Value : null,
            entityReference,
            policy,
            now ?? CreatedAt);

    private static string Snapshot(MediaAsset asset) => JsonSerializer.Serialize(asset);
}