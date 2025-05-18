using System.Reflection;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MinimalAPI.Attributes;
using MinimalAPI.Extensions;

namespace MinimalAPI;

public static partial class Register
{
    /// <summary>
    /// Adds MinimalAPI services and MediatR to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="assemblies">Assemblies to scan for endpoint handlers</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddMinimalApi(
        this IServiceCollection services,
        params Assembly[] assemblies
    )
    {
        // Register MediatR services
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(assemblies));

        // Register endpoint handlers
        foreach (var assembly in assemblies)
        {
            var endpointHandlerTypes = assembly
                .GetTypes()
                .Where(t =>
                    !t.IsAbstract && t.IsClass && t.IsAssignableTo(typeof(IEndpointHandler))
                )
                .ToList();

            foreach (var handlerType in endpointHandlerTypes)
            {
                services.AddScoped(handlerType);
            }
        }

        return services;
    }

    /// <summary>
    /// Maps all endpoints from classes implementing IEndpointHandler found in the specified assemblies
    /// </summary>
    /// <param name="app">The WebApplication</param>
    /// <param name="assemblies">Assemblies to scan for endpoint handlers</param>
    /// <returns>The WebApplication</returns>
    public static WebApplication MapMinimalEndpoints(
        this WebApplication app,
        params Assembly[] assemblies
    )
    {
        foreach (var assembly in assemblies)
        {
            app.MapEndpointsFromAssembly(assembly);
        }

        return app;
    }
}
