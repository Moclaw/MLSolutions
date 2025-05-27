using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using MinimalAPI.Attributes;
using MinimalAPI.Endpoints;

namespace MinimalAPI.SwaggerUI;

public class MinimalApiDocumentFilter : IDocumentFilter
{
    private readonly SwaggerUIOptions _options;

    public MinimalApiDocumentFilter(SwaggerUIOptions options)
    {
        _options = options;
    }

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        // Enhance document info
        swaggerDoc.Info.Title = _options.Title;
        swaggerDoc.Info.Version = _options.Version;
        swaggerDoc.Info.Description = _options.Description;

        if (!string.IsNullOrEmpty(_options.ContactName))
        {
            swaggerDoc.Info.Contact = new OpenApiContact
            {
                Name = _options.ContactName,
                Email = _options.ContactEmail,
                Url = !string.IsNullOrEmpty(_options.ContactUrl) ? new Uri(_options.ContactUrl) : null
            };
        }

        if (!string.IsNullOrEmpty(_options.LicenseName))
        {
            swaggerDoc.Info.License = new OpenApiLicense
            {
                Name = _options.LicenseName,
                Url = !string.IsNullOrEmpty(_options.LicenseUrl) ? new Uri(_options.LicenseUrl) : null
            };
        }

        // Add servers information
        swaggerDoc.Servers = new List<OpenApiServer>
        {
            new() { Url = "/", Description = "Current server" }
        };

        // Add custom tags with descriptions
        AddCustomTags(swaggerDoc);

        // Sort paths alphabetically
        var sortedPaths = swaggerDoc.Paths
            .OrderBy(p => p.Key)
            .ToDictionary(p => p.Key, p => p.Value);
        
        swaggerDoc.Paths.Clear();
        foreach (var path in sortedPaths)
        {
            swaggerDoc.Paths.Add(path.Key, path.Value);
        }
    }

    private void AddCustomTags(OpenApiDocument swaggerDoc)
    {
        swaggerDoc.Tags = new List<OpenApiTag>
        {
            new() { Name = "Todo", Description = "Todo management operations" },
            new() { Name = "Todo Commands", Description = "Operations that modify todo data (Create, Update, Delete)" },
            new() { Name = "Todo Queries", Description = "Operations that retrieve todo data (Get, List, Search)" },
            new() { Name = "Categories", Description = "Todo category management operations" },
            new() { Name = "Tags", Description = "Todo tag management operations" }
        };
    }
}
