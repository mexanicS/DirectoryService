using FileService.Infrastructure.Postgres;
using FileService.Infrastructure.Postgres.DataBase;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace FileService.IntegrationTests;

public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17")
        .WithDatabase("file_service_tests")
        .WithUsername("files_test")
        .WithPassword("files_test_password")
        .Build();

    public FileServiceDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<FileServiceDbContext>();
        options.UseFileServicePostgres(_container.GetConnectionString());
        return new FileServiceDbContext(options.Options);
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}