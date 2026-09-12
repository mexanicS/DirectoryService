using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Core.Handlers;

public static class HandlerRegistrationExtensions
{
    public static IServiceCollection AddHandlers(this IServiceCollection services, Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assembly);

        foreach (var implementation in assembly.DefinedTypes.Where(type => type.IsClass && !type.IsAbstract && !type.ContainsGenericParameters))
        {
            foreach (var contract in implementation.ImplementedInterfaces.Where(IsHandlerContract))
            {
                services.AddScoped(contract, implementation);
            }
        }

        return services;
    }

    private static bool IsHandlerContract(Type contract) =>
        contract.IsGenericType &&
        (contract.GetGenericTypeDefinition() == typeof(ICommandHandler<,>) ||
         contract.GetGenericTypeDefinition() == typeof(IQueryHandler<,>));
}
