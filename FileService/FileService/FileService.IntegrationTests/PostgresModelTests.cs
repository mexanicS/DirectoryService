using System.Text.Json;
using FileService.Domain.MediaAssets;
using FileService.Domain.StorageReferences;
using FileService.Infrastructure.Postgres;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace FileService.IntegrationTests;

public class PostgresModelTests(PostgresFixture fixture) : IClassFixture<PostgresFixture>
{
    private static readonly DateTime CreatedAt = new(2026, 11, 16, 0, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData(AssetStatus.AWAITING_UPLOAD)]
    [InlineData(AssetStatus.UPLOADING)]
    [InlineData(AssetStatus.UPLOADED)]
    [InlineData(AssetStatus.READY)]
    [InlineData(AssetStatus.FAILED)]
    [InlineData(AssetStatus.DELETED)]
    public async Task Migration_PersistsAndRestoresEveryState(AssetStatus status)
    {
        var asset = CreateAsset(status);
        await Save(asset);

        await using var context = fixture.CreateContext();
        var restored = await context.MediaAssets.IgnoreQueryFilters().AsNoTracking().SingleAsync(value => value.Id == asset.Id);

        Assert.Equal(JsonSerializer.Serialize(asset), JsonSerializer.Serialize(restored));
        Assert.Equal(DateTimeKind.Utc, restored.CreatedAt.Kind);
        Assert.Equal(DateTimeKind.Utc, restored.UpdatedAt.Kind);
    }

    [Fact]
    public async Task TrackedAggregate_CanReserveConfirmAndBecomeReadyAcrossSaves()
    {
        await using var context = fixture.CreateContext();
        var asset = CreateAsset();
        context.MediaAssets.Add(asset);
        await context.SaveChangesAsync();

        var key = Key(asset);
        Assert.True(asset.BeginUpload(key, CreatedAt.AddSeconds(1)).IsSuccess);
        await context.SaveChangesAsync();
        Assert.True(asset.ConfirmUpload(key, asset.Size, asset.ContentType,
            now: CreatedAt.AddSeconds(2)).IsSuccess);
        await context.SaveChangesAsync();
        Assert.True(asset.MarkReady(CreatedAt.AddSeconds(3)).IsSuccess);
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();
        var restored = await context.MediaAssets.SingleAsync(value => value.Id == asset.Id);

        Assert.Equal(AssetStatus.READY, restored.Status);
        Assert.True(restored.StorageReference!.IsConfirmed);
        Assert.Equal(4, restored.Version);
        Assert.Equal(1, await context.StorageReferences.CountAsync(value => value.MediaAssetId == asset.Id));
    }

    [Fact]
    public async Task OptionalComplexValues_CanBeAssignedAndMaterialized()
    {
        var asset = CreateAsset(AssetStatus.UPLOADED);
        await Save(asset);
        var reference = EntityReference.Create("course", Guid.NewGuid()).Value;
        var failure = FailureInfo.Create("processing.failed", "Processing was rejected.").Value;

        await using (var context = fixture.CreateContext())
        {
            var tracked = await context.MediaAssets.SingleAsync(value => value.Id == asset.Id);
            Assert.Null(tracked.EntityReference);
            Assert.Null(tracked.Failure);
            Assert.True(tracked.AttachToEntity(reference, CreatedAt.AddSeconds(4)).IsSuccess);
            Assert.True(tracked.MarkFailed(failure, CreatedAt.AddSeconds(5)).IsSuccess);
            await context.SaveChangesAsync();
        }

        await using var verification = fixture.CreateContext();
        var restored = await verification.MediaAssets.SingleAsync(value => value.Id == asset.Id);

        Assert.Equal(reference, restored.EntityReference);
        Assert.Equal(failure, restored.Failure);
        Assert.Null(restored.DraftId);
        Assert.False(restored.IsTemporary);
    }

    [Fact]
    public async Task EntityReference_CanBeSharedAsValueAndQueriedByTypeAndId()
    {
        var reference = EntityReference.Create("material", Guid.NewGuid()).Value;
        var first = CreateAsset(entity: reference);
        var second = CreateAsset(entity: reference);
        await using var context = fixture.CreateContext();
        context.MediaAssets.AddRange(first, second);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var restored = await context.MediaAssets.Where(asset =>
            asset.EntityReference != null && asset.EntityReference.Type == reference.Type &&
            asset.EntityReference.Id == reference.Id).ToListAsync();

        Assert.Equal(2, restored.Count);
        Assert.All(restored, asset => Assert.Equal(reference, asset.EntityReference));
    }

    [Fact]
    public async Task SoftDelete_HidesAssetAndRetainsStorageMetadata()
    {
        var asset = CreateAsset(AssetStatus.DELETED);
        await Save(asset);
        await using var context = fixture.CreateContext();

        Assert.False(await context.MediaAssets.AnyAsync(value => value.Id == asset.Id));
        Assert.True(await context.MediaAssets.IgnoreQueryFilters().AnyAsync(value => value.Id == asset.Id));
        Assert.True(await context.StorageReferences.AnyAsync(value => value.MediaAssetId == asset.Id));
    }

    [Fact]
    public async Task DuplicateStorageKey_IsRejectedAndTransactionIsRolledBack()
    {
        var first = CreateAsset();
        var second = CreateAsset();
        var key = Key(first);
        Assert.True(first.BeginUpload(key, CreatedAt.AddSeconds(1)).IsSuccess);
        Assert.True(second.BeginUpload(key, CreatedAt.AddSeconds(1)).IsSuccess);

        await using (var context = fixture.CreateContext())
        {
            context.MediaAssets.AddRange(first, second);
            var exception = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
            var postgres = Assert.IsType<PostgresException>(exception.InnerException);
            Assert.Equal(PostgresErrorCodes.UniqueViolation, postgres.SqlState);
            Assert.Equal("ux_storage_references_storage_key", postgres.ConstraintName);
        }

        await using var verification = fixture.CreateContext();
        Assert.False(await verification.MediaAssets.AnyAsync(value => value.Id == first.Id || value.Id == second.Id));
    }

    [Theory]
    [InlineData("status", "ck_media_assets_status")]
    [InlineData("kind", "ck_media_assets_kind")]
    [InlineData("usage", "ck_media_assets_usage_type")]
    [InlineData("combination", "ck_media_assets_kind_usage")]
    [InlineData("binding", "ck_media_assets_binding")]
    [InlineData("size", "ck_media_assets_size")]
    public async Task Database_RejectsInvalidStateAndClassification(string invalidPart, string constraint)
    {
        var asset = CreateAsset();
        await Save(asset);
        await using var context = fixture.CreateContext();
        FormattableString command = invalidPart switch
        {
            "status" => $"UPDATE files.media_assets SET status = 99 WHERE id = {asset.Id.Value}",
            "kind" => $"UPDATE files.media_assets SET kind = 99 WHERE id = {asset.Id.Value}",
            "usage" => $"UPDATE files.media_assets SET usage_type = 99 WHERE id = {asset.Id.Value}",
            "combination" => $"UPDATE files.media_assets SET kind = 2 WHERE id = {asset.Id.Value}",
            "binding" => $"UPDATE files.media_assets SET draft_id = NULL WHERE id = {asset.Id.Value}",
            "size" => $"UPDATE files.media_assets SET size_bytes = 0 WHERE id = {asset.Id.Value}",
            _ => throw new ArgumentOutOfRangeException(nameof(invalidPart)),
        };

        var exception = await Assert.ThrowsAsync<PostgresException>(() => context.Database.ExecuteSqlInterpolatedAsync(command));

        Assert.Equal(PostgresErrorCodes.CheckViolation, exception.SqlState);
        if (invalidPart is "kind" or "usage")
        {
            Assert.Contains(exception.ConstraintName, new[] { constraint, "ck_media_assets_kind_usage" });
        }
        else
        {
            Assert.Equal(constraint, exception.ConstraintName);
        }
    }

    [Fact]
    public async Task Database_RejectsIncompleteConfirmationAndPathTraversal()
    {
        var asset = CreateAsset(AssetStatus.UPLOADING);
        await Save(asset);
        await using var context = fixture.CreateContext();

        var confirmation = await Assert.ThrowsAsync<PostgresException>(() => context.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE files.storage_references SET confirmed_at = {CreatedAt.AddSeconds(2)} WHERE media_asset_id = {asset.Id.Value}"));
        var key = await Assert.ThrowsAsync<PostgresException>(() => context.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE files.storage_references SET storage_key = {"../unsafe"} WHERE media_asset_id = {asset.Id.Value}"));

        Assert.Equal("ck_storage_references_confirmation", confirmation.ConstraintName);
        Assert.Equal("ck_storage_references_key", key.ConstraintName);
    }

    [Fact]
    public async Task ConcurrentUpdate_CannotOverwriteAnotherTransition()
    {
        var asset = CreateAsset();
        await Save(asset);
        await using var first = fixture.CreateContext();
        await using var second = fixture.CreateContext();
        var winner = await first.MediaAssets.SingleAsync(value => value.Id == asset.Id);
        var stale = await second.MediaAssets.SingleAsync(value => value.Id == asset.Id);

        Assert.True(winner.BeginUpload(Key(winner), CreatedAt.AddSeconds(1)).IsSuccess);
        await first.SaveChangesAsync();
        Assert.True(stale.Delete(CreatedAt.AddSeconds(2)).IsSuccess);

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => second.SaveChangesAsync());
        await using var verification = fixture.CreateContext();
        var restored = await verification.MediaAssets.SingleAsync(value => value.Id == asset.Id);
        Assert.Equal(AssetStatus.UPLOADING, restored.Status);
        Assert.Equal(2, restored.Version);
        Assert.NotNull(restored.StorageReference);
    }

    [Fact]
    public async Task Migration_CreatesSchemaIndexesAndOwnHistoryWithoutBinaryColumns()
    {
        await using var context = fixture.CreateContext();
        var connection = context.Database.GetDbConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT indexname FROM pg_indexes WHERE schemaname = 'files'";
        var indexes = new List<string>();
        await using (var reader = await command.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                indexes.Add(reader.GetString(0));
            }
        }

        Assert.Contains("ix_media_assets_status_updated_at", indexes);
        Assert.Contains("ix_media_assets_usage_status_created_at", indexes);
        Assert.Contains("ix_media_assets_draft_created_at", indexes);
        Assert.Contains("ix_media_assets_entity_usage", indexes);
        Assert.Contains("ux_storage_references_storage_key", indexes);

        command.CommandText = "SELECT count(*) FROM files.__ef_migrations_history";
        Assert.Equal(1L, await command.ExecuteScalarAsync());
        command.CommandText = "SELECT count(*) FROM information_schema.columns WHERE table_schema = 'files' AND data_type = 'bytea'";
        Assert.Equal(0L, await command.ExecuteScalarAsync());
        Assert.False(context.Database.HasPendingModelChanges());
        Assert.Null(context.Model.FindEntityType(typeof(MediaAsset))!.FindDiscriminatorProperty());
    }

    private static MediaAsset CreateAsset(
        AssetStatus status = AssetStatus.AWAITING_UPLOAD, EntityReference? entity = null)
    {
        var asset = MediaAsset.CreateForUpload(
            MediaKind.IMAGE,
            UsageType.AVATAR,
            FileName.Create("Аватар.PNG").Value,
            MimeType.Create("image/png").Value,
            FileSize.Create(100).Value,
            UploaderId.Create(Guid.NewGuid()).Value,
            entity is null ? DraftId.Create(Guid.NewGuid()).Value : null,
            entity,
            now: CreatedAt).Value;

        if (status == AssetStatus.AWAITING_UPLOAD)
        {
            return asset;
        }

        var key = Key(asset);
        Assert.True(asset.BeginUpload(key, CreatedAt.AddSeconds(1)).IsSuccess);
        if (status == AssetStatus.UPLOADING)
        {
            return asset;
        }

        Assert.True(asset.ConfirmUpload(key, asset.Size, asset.ContentType,
            Sha256Checksum.Create(new string('a', 64)).Value, ETag.Create("\"test-etag\"").Value,
            now: CreatedAt.AddSeconds(2)).IsSuccess);
        if (status == AssetStatus.UPLOADED)
        {
            return asset;
        }

        if (status == AssetStatus.FAILED)
        {
            var failure = FailureInfo.Create("upload.failed", "Upload failed.").Value;
            Assert.True(asset.MarkFailed(failure, CreatedAt.AddSeconds(3)).IsSuccess);
            return asset;
        }

        Assert.True(asset.MarkReady(CreatedAt.AddSeconds(3)).IsSuccess);
        if (status == AssetStatus.DELETED)
        {
            Assert.True(asset.Delete(CreatedAt.AddSeconds(4)).IsSuccess);
        }

        return asset;
    }

    private static StorageKey Key(MediaAsset asset) => StorageKey.Create($"originals/{asset.Id.Value:N}/image.png").Value;

    private async Task Save(MediaAsset asset)
    {
        await using var context = fixture.CreateContext();
        context.MediaAssets.Add(asset);
        await context.SaveChangesAsync();
    }
}