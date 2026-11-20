using FileService.Infrastructure.Postgres.DataBase;
using FileService.Infrastructure.Postgres.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FileService.Infrastructure.Postgres;

public static class Inject
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<PostgresOptions>()
            .Bind(configuration.GetSection(PostgresOptions.SECTION_NAME))
            .Validate(
                options => PostgresOptions.IsValidConnectionString(options.Database),
                "ConnectionStrings:Database must contain a valid PostgreSQL connection string with Host and Database.")
            .ValidateOnStart();

        services.AddDbContext<FileServiceDbContext>((provider, options) =>
            options.UseFileServicePostgres(provider.GetRequiredService<IOptions<PostgresOptions>>().Value.Database));

        return services;
    }
}