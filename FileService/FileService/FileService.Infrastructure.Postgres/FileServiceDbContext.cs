using FileService.Domain.MediaAssets;
using FileService.Domain.StorageReferences;
using Microsoft.EntityFrameworkCore;

namespace FileService.Infrastructure.Postgres;

public class FileServiceDbContext(DbContextOptions<FileServiceDbContext> options) : DbContext(options)
{
    public const string SCHEMA = "files";
    public const string MIGRATIONS_HISTORY_TABLE = "__ef_migrations_history";

    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();

    public DbSet<StorageReference> StorageReferences => Set<StorageReference>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(SCHEMA);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FileServiceDbContext).Assembly);
    }
}