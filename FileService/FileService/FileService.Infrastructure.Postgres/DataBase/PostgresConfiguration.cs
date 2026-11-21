using Microsoft.EntityFrameworkCore;

namespace FileService.Infrastructure.Postgres.DataBase;

public static class PostgresConfiguration
{
    public static DbContextOptionsBuilder UseFileServicePostgres(
        this DbContextOptionsBuilder options, string connectionString) =>
        options.UseNpgsql(connectionString, postgres =>
            postgres.MigrationsHistoryTable(FileServiceDbContext.MIGRATIONS_HISTORY_TABLE, FileServiceDbContext.SCHEMA));
}