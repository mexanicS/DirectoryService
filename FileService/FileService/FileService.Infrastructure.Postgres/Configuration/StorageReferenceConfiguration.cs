using FileService.Domain.MediaAssets;
using FileService.Domain.Shared;
using FileService.Domain.StorageReferences;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FileService.Infrastructure.Postgres.Configuration;

public class StorageReferenceConfiguration : IEntityTypeConfiguration<StorageReference>
{
    public void Configure(EntityTypeBuilder<StorageReference> builder)
    {
        builder.ToTable("storage_references", table =>
        {
            table.HasCheckConstraint(
                "ck_storage_references_key",
                @"storage_key ~ '^(?:[a-z0-9][a-z0-9_-]*/)*[a-z0-9][a-z0-9_-]*(?:\.[a-z0-9]+)?$'");
            table.HasCheckConstraint("ck_storage_references_size", "actual_size_bytes IS NULL OR actual_size_bytes > 0");
            table.HasCheckConstraint(
                "ck_storage_references_checksum",
                "checksum_sha256 IS NULL OR checksum_sha256 ~ '^[0-9a-f]{64}$'");
            table.HasCheckConstraint(
                "ck_storage_references_etag",
                "etag IS NULL OR (length(btrim(etag)) > 0 AND etag !~ '[[:cntrl:]]')");
            table.HasCheckConstraint(
                "ck_storage_references_confirmation",
                "(confirmed_at IS NULL AND actual_size_bytes IS NULL AND detected_content_type IS NULL AND " +
                "checksum_sha256 IS NULL AND etag IS NULL) OR " +
                "(confirmed_at IS NOT NULL AND actual_size_bytes IS NOT NULL AND detected_content_type IS NOT NULL)");
            table.HasCheckConstraint(
                "ck_storage_references_timestamps",
                "confirmed_at IS NULL OR confirmed_at >= reserved_at");
        });

        builder.HasKey(reference => reference.MediaAssetId).HasName("pk_storage_references");
        builder.Property(reference => reference.MediaAssetId)
            .HasColumnName("media_asset_id")
            .HasConversion(id => id.Value, value => MediaAssetId.Create(value).Value)
            .ValueGeneratedNever();

        builder.Property(reference => reference.Key)
            .HasColumnName("storage_key")
            .HasMaxLength(Constants.MAX_LENGTH_STORAGE_KEY)
            .HasConversion(key => key.Value, value => StorageKey.Create(value).Value)
            .IsRequired();

        builder.Property(reference => reference.ActualSize)
            .HasColumnName("actual_size_bytes")
            .HasConversion(size => size!.Value, value => FileSize.Create(value).Value)
            .IsRequired(false);

        builder.Property(reference => reference.DetectedContentType)
            .HasColumnName("detected_content_type")
            .HasMaxLength(Constants.MAX_LENGTH_MIME_TYPE)
            .HasConversion(type => type!.Value, value => MimeType.Create(value).Value)
            .IsRequired(false);

        builder.Property(reference => reference.Checksum)
            .HasColumnName("checksum_sha256")
            .HasMaxLength(Constants.SHA256_LENGTH)
            .HasConversion(checksum => checksum!.Value, value => Sha256Checksum.Create(value).Value)
            .IsRequired(false);

        builder.Property(reference => reference.ETag)
            .HasColumnName("etag")
            .HasMaxLength(Constants.MAX_LENGTH_ETAG)
            .HasConversion(etag => etag!.Value, value => ETag.Create(value).Value)
            .IsRequired(false);

        builder.Property(reference => reference.ReservedAt).HasColumnName("reserved_at").HasColumnType("timestamp with time zone");
        builder.Property(reference => reference.ConfirmedAt).HasColumnName("confirmed_at").HasColumnType("timestamp with time zone");
        builder.Ignore(reference => reference.IsConfirmed);

        builder.HasIndex(reference => reference.Key)
            .IsUnique()
            .HasDatabaseName("ux_storage_references_storage_key");
    }
}