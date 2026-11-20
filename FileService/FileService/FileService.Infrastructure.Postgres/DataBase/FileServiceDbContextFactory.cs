using FileService.Infrastructure.Postgres.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FileService.Infrastructure.Postgres.DataBase;

public class FileServiceDbContextFactory : IDesignTimeDbContextFactory<FileServiceDbContext>
{
    public FileServiceDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Database");
        if (!PostgresOptions.IsValidConnectionString(connectionString))
        {
            throw new InvalidOperationException("Set ConnectionStrings__Database to a valid PostgreSQL connection string.");
        }

        var options = new DbContextOptionsBuilder<FileServiceDbContext>();
        options.UseFileServicePostgres(connectionString!);
        return new FileServiceDbContext(options.Options);
    }
}