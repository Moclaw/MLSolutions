using Host;
using Host.Services;
using Microsoft.EntityFrameworkCore;
using MinimalAPI;
using MinimalAPI.Extensions;
using MinimalAPI.OpenApi;
using sample.Application;
using sample.Infrastructure;
using sample.Infrastructure.Persistence.EfCore;

var builder = WebApplication.CreateBuilder(args);

var appName = builder.Environment.ApplicationName;
var configuration = builder.Configuration;

// Configure Serilog
builder.AddSerilog(configuration, appName);

// Configure versioning options with enhanced settings
var versioningOptions = new DefaultVersioningOptions
{
    Prefix = "v",
    DefaultVersion = 1,
    SupportedVersions = [1, 2],
    IncludeVersionInRoute = true,
    BaseRouteTemplate = "/api",
    ReadingStrategy = VersionReadingStrategy.UrlSegment | VersionReadingStrategy.QueryString,
    AssumeDefaultVersionWhenUnspecified = true,
    QueryParameterName = "version",
    VersionHeaderName = "X-API-Version",
    
    // Enhanced SwaggerUI settings
    GenerateSwaggerDocs = true,
    SwaggerDocTitle = "Todo API",
    SwaggerDocDescription = "Comprehensive API for Todo Management with CRUD operations, built using MinimalAPI framework with MediatR and CQRS pattern"
};

// Register other services
builder.Services
    .AddCorsServices(configuration)
    .AddMinimalApiWithSwaggerUI(
        title: "Todo API",
        version: "v1",
        description: "Comprehensive API for Todo Management with CRUD operations, built using MinimalAPI framework with MediatR and CQRS pattern",
        contactName: "MLSolutions Development Team",
        contactEmail: "dev@mlsolutions.com",
        contactUrl: "https://mlsolutions.com/support",
        licenseName: "MIT License",
        licenseUrl: "https://opensource.org/licenses/MIT",
        versioningOptions: versioningOptions,
        assemblies: [
            typeof(Program).Assembly,
            typeof(sample.Application.Register).Assembly,
            typeof(sample.Infrastructure.Register).Assembly
        ]
    )
    .AddGlobalExceptionHandling(appName)
    .AddHealthCheck(configuration)
    // Register Infrastructure and Application services
    .AddInfrastructureServices(configuration)
    .AddApplicationServices(configuration);

var app = builder.Build();

// Map all endpoints with versioning support
app.MapMinimalEndpoints(versioningOptions, typeof(Program).Assembly);

// Enable enhanced documentation with SwaggerUI and versioning
if (app.Environment.IsDevelopment())
{
    // OpenAPI documentation for MinimalAPI endpoints with versioning
    app.UseMinimalApiOpenApi();

    app.UseMinimalApiDocs(
        swaggerRoutePrefix: "docs",
        enableTryItOut: true,
        enableDeepLinking: true,
        enableFilter: true
    );
}

app.UseHttpsRedirection();

// Configure CORS
app.UseCorsServices(configuration);

// Configure Global Exception Handling
app.UseGlobalExceptionHandling();

// Configure ARM Elastic
app.UseElasticApm(configuration);

app.UseRouting();

// Configure Health Check
app.UseHealthChecks(configuration);

await app.RunAsync();
