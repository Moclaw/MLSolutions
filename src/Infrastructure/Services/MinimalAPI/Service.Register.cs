using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

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
        // Register MediatR handlers from all assemblies
        // Find all potential application assemblies that might contain handlers
        var allAssemblies = new List<Assembly>(assemblies);

        // Add references to application assemblies that contain handlers
        foreach (var assembly in assemblies)
        {
            var referencedAssemblies = assembly
                .GetReferencedAssemblies()
                .Where(
                    a =>
                        a.Name?.Contains("Application") == true
                        || a.Name?.Contains("Features") == true
                        || a.Name?.Contains("Handlers") == true
                )
                .Select(Assembly.Load);

            allAssemblies.AddRange(referencedAssemblies);
        }

        // Register MediatR services
        services.AddMediatR(
            cfg => cfg.RegisterServicesFromAssemblies(allAssemblies.Distinct().ToArray())
        );

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
        // Find all endpoint classes (classes deriving from EndpointBase)
        var endpointTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(
                t =>
                    !t.IsAbstract
                    && !t.IsInterface
                    && t.IsAssignableTo(typeof(MinimalAPI.Endpoints.EndpointAbstractBase))
            )
            .ToList();

        foreach (var endpointType in endpointTypes)
        {
            // Get route attribute
            var routeAttr = endpointType.GetCustomAttribute<MinimalAPI.Attributes.RouteAttribute>();
            if (routeAttr == null)
                continue;

            string basePath = routeAttr.Template;

            // Find all handler methods with HTTP method attributes
            var methods = endpointType
                .GetMethods()
                .Where(
                    m =>
                        m.GetCustomAttributes()
                            .Any(a => a is MinimalAPI.Attributes.HttpMethodAttribute)
                )
                .ToList();

            foreach (var method in methods)
            {
                // Get HTTP method attribute
                var httpMethodAttr =
                    method
                        .GetCustomAttributes()
                        .FirstOrDefault(a => a is MinimalAPI.Attributes.HttpMethodAttribute)
                    as MinimalAPI.Attributes.HttpMethodAttribute;

                if (httpMethodAttr == null)
                    continue;

                // Create the full route template
                string routeTemplate = basePath;
                if (!string.IsNullOrEmpty(httpMethodAttr.Route))
                {
                    routeTemplate = string.IsNullOrEmpty(routeTemplate)
                        ? httpMethodAttr.Route
                        : $"{routeTemplate}/{httpMethodAttr.Route}";
                }

                // Create the endpoint handler
                var handler = app.MapMethods(
                    routeTemplate,
                    new[] { httpMethodAttr.Method },
                    async (HttpContext context, IServiceProvider serviceProvider) =>
                    {
                        // Create endpoint instance from service provider
                        var endpoint =
                            ActivatorUtilities.CreateInstance(serviceProvider, endpointType)
                            as MinimalAPI.Endpoints.EndpointAbstractBase;
                        if (endpoint == null)
                            return Results.StatusCode(500);

                        // Set HttpContext on the endpoint
                        endpoint.HttpContext = context;

                        // Execute the endpoint
                        await endpoint.ExecuteAsync(context.RequestAborted);

                        // Return an empty result as the endpoint will have written to the response
                        return Results.Empty;
                    }
                );
            }
        }

        return app;
    }
}
