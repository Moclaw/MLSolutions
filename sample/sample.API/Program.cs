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

// Configure versioning options
var versioningOptions = new DefaultVersioningOptions
{
    Prefix = "v",
    DefaultVersion = 1,
};

// Register other services
builder.Services
    .AddCorsServices(configuration)
    .AddMinimalApiWithSwaggerUI(
        title: "Todo API",
        version: "v1",
        description: "Comprehensive API for Todo Management with CRUD operations, built using MinimalAPI framework with MediatR and CQRS pattern",
        contactName: "Todo API Development Team",
        contactEmail: "dev@todoapi.com",
        contactUrl: "https://github.com/Moclaw/MLSolutions",
        licenseName: "MIT",
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

// Map all endpoints from the assembly with versioning BEFORE Swagger configuration
app.MapMinimalEndpoints(versioningOptions, typeof(Program).Assembly);

// Enable enhanced documentation with SwaggerUI
if (app.Environment.IsDevelopment())
{
    // OpenAPI documentation for MinimalAPI endpoints: openapi/v1.json
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
