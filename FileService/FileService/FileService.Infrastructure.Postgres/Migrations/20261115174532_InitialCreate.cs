using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileService.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "files");

            migrationBuilder.CreateTable(
                name: "media_assets",
                schema: "files",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<short>(type: "smallint", nullable: false),
                    usage_type = table.Column<short>(type: "smallint", nullable: false),
                    original_file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    content_type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    uploader_id = table.Column<Guid>(type: "uuid", nullable: false),
                    draft_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    failed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    soft_deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<long>(type: "bigint", nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: true),
                    entity_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    failure_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    failure_message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_media_assets", x => x.id);
                    table.CheckConstraint("ck_media_assets_binding", "(draft_id IS NOT NULL AND entity_type IS NULL AND entity_id IS NULL) OR (draft_id IS NULL AND entity_type IS NOT NULL AND entity_id IS NOT NULL)");
                    table.CheckConstraint("ck_media_assets_deletion", "(status = 6 AND soft_deleted_at IS NOT NULL) OR (status <> 6 AND soft_deleted_at IS NULL)");
                    table.CheckConstraint("ck_media_assets_entity_type", "entity_type IS NULL OR entity_type ~ '^[a-z][a-z0-9_-]{0,63}$'");
                    table.CheckConstraint("ck_media_assets_failure", "(failure_code IS NULL AND failure_message IS NULL AND failed_at IS NULL AND status <> 5) OR (failure_code IS NOT NULL AND failure_message IS NOT NULL AND failed_at IS NOT NULL AND status IN (5, 6))");
                    table.CheckConstraint("ck_media_assets_failure_values", "failure_code IS NULL OR (failure_code ~ '^[a-z0-9][a-z0-9._-]{0,99}$' AND length(btrim(failure_message)) > 0)");
                    table.CheckConstraint("ck_media_assets_identifiers", "id <> '00000000-0000-0000-0000-000000000000'::uuid AND uploader_id <> '00000000-0000-0000-0000-000000000000'::uuid AND (draft_id IS NULL OR draft_id <> '00000000-0000-0000-0000-000000000000'::uuid) AND (entity_id IS NULL OR entity_id <> '00000000-0000-0000-0000-000000000000'::uuid)");
                    table.CheckConstraint("ck_media_assets_kind", "kind IN (1, 2, 3)");
                    table.CheckConstraint("ck_media_assets_kind_usage", "(kind = 1 AND usage_type IN (1, 2, 3, 6)) OR (kind = 2 AND usage_type = 4) OR (kind = 3 AND usage_type = 5)");
                    table.CheckConstraint("ck_media_assets_size", "size_bytes > 0");
                    table.CheckConstraint("ck_media_assets_status", "status IN (1, 2, 3, 4, 5, 6)");
                    table.CheckConstraint("ck_media_assets_timestamps", "updated_at >= created_at AND (failed_at IS NULL OR (failed_at >= created_at AND failed_at <= updated_at)) AND (soft_deleted_at IS NULL OR soft_deleted_at = updated_at)");
                    table.CheckConstraint("ck_media_assets_usage_type", "usage_type IN (1, 2, 3, 4, 5, 6)");
                    table.CheckConstraint("ck_media_assets_version", "version > 0");
                });

            migrationBuilder.CreateTable(
                name: "storage_references",
                schema: "files",
                columns: table => new
                {
                    media_asset_id = table.Column<Guid>(type: "uuid", nullable: false),
                    storage_key = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    reserved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    actual_size_bytes = table.Column<long>(type: "bigint", nullable: true),
                    detected_content_type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    checksum_sha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    etag = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    confirmed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_storage_references", x => x.media_asset_id);
                    table.CheckConstraint("ck_storage_references_checksum", "checksum_sha256 IS NULL OR checksum_sha256 ~ '^[0-9a-f]{64}$'");
                    table.CheckConstraint("ck_storage_references_confirmation", "(confirmed_at IS NULL AND actual_size_bytes IS NULL AND detected_content_type IS NULL AND checksum_sha256 IS NULL AND etag IS NULL) OR (confirmed_at IS NOT NULL AND actual_size_bytes IS NOT NULL AND detected_content_type IS NOT NULL)");
                    table.CheckConstraint("ck_storage_references_etag", "etag IS NULL OR (length(btrim(etag)) > 0 AND etag !~ '[[:cntrl:]]')");
                    table.CheckConstraint("ck_storage_references_key", "storage_key ~ '^(?:[a-z0-9][a-z0-9_-]*/)*[a-z0-9][a-z0-9_-]*(?:\\.[a-z0-9]+)?$'");
                    table.CheckConstraint("ck_storage_references_size", "actual_size_bytes IS NULL OR actual_size_bytes > 0");
                    table.CheckConstraint("ck_storage_references_timestamps", "confirmed_at IS NULL OR confirmed_at >= reserved_at");
                    table.ForeignKey(
                        name: "fk_storage_references_media_assets",
                        column: x => x.media_asset_id,
                        principalSchema: "files",
                        principalTable: "media_assets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            // EF Core 10 does not support HasIndex over complex properties.
            migrationBuilder.CreateIndex(
                name: "ix_media_assets_entity_usage",
                schema: "files",
                table: "media_assets",
                columns: new[] { "entity_type", "entity_id", "usage_type" },
                filter: "entity_id IS NOT NULL AND soft_deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_media_assets_draft_created_at",
                schema: "files",
                table: "media_assets",
                columns: new[] { "draft_id", "created_at" },
                filter: "draft_id IS NOT NULL AND soft_deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_media_assets_status_updated_at",
                schema: "files",
                table: "media_assets",
                columns: new[] { "status", "updated_at" });

            migrationBuilder.CreateIndex(
                name: "ix_media_assets_uploader_created_at",
                schema: "files",
                table: "media_assets",
                columns: new[] { "uploader_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_media_assets_usage_status_created_at",
                schema: "files",
                table: "media_assets",
                columns: new[] { "usage_type", "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ux_storage_references_storage_key",
                schema: "files",
                table: "storage_references",
                column: "storage_key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "storage_references",
                schema: "files");

            migrationBuilder.DropTable(
                name: "media_assets",
                schema: "files");
        }
    }
}