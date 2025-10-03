using DirectoryService.Application.DirectoryServiceManagement.Commands.Locations;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Application;

public static class Inject
{
    public static IServiceCollection AddSpeciesApplication(this IServiceCollection services)
    {
        services.AddScoped<CreateLocationHandler>();
        services.AddValidatorsFromAssembly(typeof(Inject).Assembly);
        
        return services;
    }
}