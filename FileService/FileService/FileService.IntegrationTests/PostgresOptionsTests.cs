using FileService.Infrastructure.Postgres;
using FileService.Infrastructure.Postgres.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FileService.IntegrationTests;

public class PostgresOptionsTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-connection-string")]
    [InlineData("Host=localhost")]
    [InlineData("Database=files")]
    public void Configuration_RejectsMissingOrInvalidConnectionString(string? value)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:Database"] = value })
            .Build();
        var services = new ServiceCollection();
        services.AddInfrastructure(configuration);
        using var provider = services.BuildServiceProvider();

        Assert.Throws<OptionsValidationException>(() =>
            provider.GetRequiredService<IOptions<PostgresOptions>>().Value);
    }

    [Fact]
    public void Configuration_RegistersContextWithSameHistorySchemaAsDesignTime()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Database"] = "Host=localhost;Database=files_test;Username=files_test",
            })
            .Build();
        var services = new ServiceCollection();
        services.AddInfrastructure(configuration);
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<FileServiceDbContext>();

        Assert.Equal("files", context.Model.GetDefaultSchema());
        Assert.Equal("Npgsql.EntityFrameworkCore.PostgreSQL", context.Database.ProviderName);
    }
}