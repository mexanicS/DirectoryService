using DirectoryService.Application.DirectoryServiceManagement.Commands.Locations;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Application;

public static class Inject
{
    public static IServiceCollection AddSpeciesApplication(this IServiceCollection services)
    {
        services.AddScoped<CreateLocationHandler>();
        
        return services;
    }
}