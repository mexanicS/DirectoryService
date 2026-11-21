using FileService.Domain.MediaAssets;
using FileService.Domain.Shared;
using FileService.Domain.StorageReferences;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FileService.Infrastructure.Postgres.Configuration;

public class MediaAssetConfiguration : IEntityTypeConfiguration<MediaAsset>
{
    public void Configure(EntityTypeBuilder<MediaAsset> builder)
    {
        builder.ToTable("media_assets", table =>
        {
            table.HasCheckConstraint("ck_media_assets_kind", "kind IN (1, 2, 3)");
            table.HasCheckConstraint("ck_media_assets_usage_type", "usage_type IN (1, 2, 3, 4, 5, 6)");
            table.HasCheckConstraint("ck_media_assets_status", "status IN (1, 2, 3, 4, 5, 6)");
            table.HasCheckConstraint(
                "ck_media_assets_kind_usage",
                "(kind = 1 AND usage_type IN (1, 2, 3, 6)) OR " +
                "(kind = 2 AND usage_type = 4) OR (kind = 3 AND usage_type = 5)");
            table.HasCheckConstraint("ck_media_assets_size", "size_bytes > 0");
            table.HasCheckConstraint("ck_media_assets_version", "version > 0");
            table.HasCheckConstraint(
                "ck_media_assets_binding",
                "(draft_id IS NOT NULL AND entity_type IS NULL AND entity_id IS NULL) OR " +
                "(draft_id IS NULL AND entity_type IS NOT NULL AND entity_id IS NOT NULL)");
            table.HasCheckConstraint(
                "ck_media_assets_identifiers",
                "id <> '00000000-0000-0000-0000-000000000000'::uuid AND " +
                "uploader_id <> '00000000-0000-0000-0000-000000000000'::uuid AND " +
                "(draft_id IS NULL OR draft_id <> '00000000-0000-0000-0000-000000000000'::uuid) AND " +
                "(entity_id IS NULL OR entity_id <> '00000000-0000-0000-0000-000000000000'::uuid)");
            table.HasCheckConstraint(
                "ck_media_assets_entity_type",
                "entity_type IS NULL OR entity_type ~ '^[a-z][a-z0-9_-]{0,63}$'");
            table.HasCheckConstraint(
                "ck_media_assets_deletion",
                "(status = 6 AND soft_deleted_at IS NOT NULL) OR (status <> 6 AND soft_deleted_at IS NULL)");
            table.HasCheckConstraint(
                "ck_media_assets_failure",
                "(failure_code IS NULL AND failure_message IS NULL AND failed_at IS NULL AND status <> 5) OR " +
                "(failure_code IS NOT NULL AND failure_message IS NOT NULL AND failed_at IS NOT NULL AND status IN (5, 6))");
            table.HasCheckConstraint(
                "ck_media_assets_failure_values",
                "failure_code IS NULL OR (failure_code ~ '^[a-z0-9][a-z0-9._-]{0,99}$' AND length(btrim(failure_message)) > 0)");
            table.HasCheckConstraint(
                "ck_media_assets_timestamps",
                "updated_at >= created_at AND (failed_at IS NULL OR (failed_at >= created_at AND failed_at <= updated_at)) AND " +
                "(soft_deleted_at IS NULL OR soft_deleted_at = updated_at)");
        });

        builder.HasKey(asset => asset.Id).HasName("pk_media_assets");
        builder.Property(asset => asset.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => MediaAssetId.Create(value).Value)
            .ValueGeneratedNever();

        builder.Property(asset => asset.Kind).HasColumnName("kind").HasConversion<short>().IsRequired();
        builder.Property(asset => asset.UsageType).HasColumnName("usage_type").HasConversion<short>().IsRequired();
        builder.Property(asset => asset.Status).HasColumnName("status").HasConversion<short>().IsRequired();

        builder.Property(asset => asset.OriginalFileName)
            .HasColumnName("original_file_name")
            .HasMaxLength(Constants.MAX_LENGTH_FILE_NAME)
            .HasConversion(name => name.Value, value => FileName.Create(value).Value)
            .IsRequired();

        builder.Property(asset => asset.ContentType)
            .HasColumnName("content_type")
            .HasMaxLength(Constants.MAX_LENGTH_MIME_TYPE)
            .HasConversion(type => type.Value, value => MimeType.Create(value).Value)
            .IsRequired();

        builder.Property(asset => asset.Size)
            .HasColumnName("size_bytes")
            .HasConversion(size => size.Value, value => FileSize.Create(value).Value)
            .IsRequired();

        builder.Property(asset => asset.UploadedBy)
            .HasColumnName("uploader_id")
            .HasConversion(id => id.Value, value => UploaderId.Create(value).Value)
            .IsRequired();

        builder.Property(asset => asset.DraftId)
            .HasColumnName("draft_id")
            .HasConversion(id => id!.Value, value => DraftId.Create(value).Value)
            .IsRequired(false);

        var reference = builder.ComplexProperty(asset => asset.EntityReference);
        reference.IsRequired(false);
        reference.Property(value => value!.Type)
            .HasColumnName("entity_type")
            .HasMaxLength(Constants.MAX_LENGTH_ENTITY_TYPE)
            .IsRequired();
        reference.Property(value => value!.Id).HasColumnName("entity_id").IsRequired();

        var failure = builder.ComplexProperty(asset => asset.Failure);
        failure.IsRequired(false);
        failure.Property(value => value!.Code)
            .HasColumnName("failure_code")
            .HasMaxLength(Constants.MAX_LENGTH_FAILURE_CODE)
            .IsRequired();
        failure.Property(value => value!.Message)
            .HasColumnName("failure_message")
            .HasMaxLength(Constants.MAX_LENGTH_FAILURE_MESSAGE)
            .IsRequired();

        builder.Property(asset => asset.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
        builder.Property(asset => asset.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");
        builder.Property(asset => asset.FailedAt).HasColumnName("failed_at").HasColumnType("timestamp with time zone");
        builder.Property(asset => asset.SoftDeletedAt).HasColumnName("soft_deleted_at").HasColumnType("timestamp with time zone");
        builder.Property(asset => asset.Version).HasColumnName("version").IsConcurrencyToken();

        builder.Ignore(asset => asset.IsTemporary);
        builder.Ignore(asset => asset.IsDeleted);
        builder.HasQueryFilter(asset => asset.Status != AssetStatus.DELETED);

        builder.HasOne(asset => asset.StorageReference)
            .WithOne()
            .HasForeignKey<StorageReference>(storage => storage.MediaAssetId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_storage_references_media_assets");
        builder.Navigation(asset => asset.StorageReference).AutoInclude();

        builder.HasIndex(asset => new { asset.Status, asset.UpdatedAt })
            .HasDatabaseName("ix_media_assets_status_updated_at");
        builder.HasIndex(asset => new { asset.UsageType, asset.Status, asset.CreatedAt })
            .HasDatabaseName("ix_media_assets_usage_status_created_at");
        builder.HasIndex(asset => new { asset.DraftId, asset.CreatedAt })
            .HasFilter("draft_id IS NOT NULL AND soft_deleted_at IS NULL")
            .HasDatabaseName("ix_media_assets_draft_created_at");
        builder.HasIndex(asset => new { asset.UploadedBy, asset.CreatedAt })
            .HasDatabaseName("ix_media_assets_uploader_created_at");

        // EF Core 10 cannot model indexes over complex properties.
        // The entity_type/entity_id/usage_type index is created explicitly in InitialCreate.
    }
}