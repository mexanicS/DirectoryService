using DirectoryService.Application.DirectoryServiceManagement.Commands;
using DirectoryService.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Infrastructure;

public static class Inject
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddDbContexts(configuration)
            .AddRepositories();
        
        return services;
    }
    
    private static IServiceCollection AddDbContexts(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<DirectoryServiceDbContext>(_ => 
            new DirectoryServiceDbContext(configuration.GetConnectionString("Database")!));
        
        return services;
    }
    
    private static IServiceCollection AddRepositories(
        this IServiceCollection services)
    {
        services.AddScoped<ILocationsRepository, LocationsRepository>();
        return services;
    }
}