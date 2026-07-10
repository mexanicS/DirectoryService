using DirectoryService.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace DirectoryService.IntegrationTests;

public class DirectoryTestWebFactory : WebApplicationFactory<Presentation.Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres")
        .WithDatabase("directory_service_db")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private Respawner _respawner = null!;
    private NpgsqlConnection _connection = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DirectoryServiceDbContext>();
            
            services.AddScoped<DirectoryServiceDbContext>(_ => 
                new DirectoryServiceDbContext(_container.GetConnectionString()));

            services.RemoveAll<Application.Database.ISqlConnectionFactory>();
            services.AddScoped<Application.Database.ISqlConnectionFactory>(_ => 
                new TestSqlConnectionFactory(_container.GetConnectionString()));
        });
    }

    private class TestSqlConnectionFactory(string connectionString) : Application.Database.ISqlConnectionFactory
    {
        public System.Data.IDbConnection Create()
        {
            return new NpgsqlConnection(connectionString);
        }
    }
    
    
    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DirectoryServiceDbContext>();

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
        
        _connection =  new NpgsqlConnection(_container.GetConnectionString());
        await _connection.OpenAsync();
        
        await InitRespawner();
        
    }

    public new async Task DisposeAsync()
    {
        await _container.StopAsync();
        await _container.DisposeAsync();

        await _connection.CloseAsync();
        await _connection.DisposeAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_connection);
    }

    private async Task InitRespawner()
    {
        _respawner = await Respawner.CreateAsync(
            _connection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = ["DirectoryService"],
            });
    }
}