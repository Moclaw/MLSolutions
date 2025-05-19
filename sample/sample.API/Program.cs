using Host;
using Host.Services;
using MinimalAPI;
using sample.Application;
using sample.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var appName = builder.Environment.ApplicationName;
var configuration = builder.Configuration;

// Configure Serilog
builder.AddSerilog(configuration, appName);

// Register other services
builder
    .Services.AddCorsServices(configuration)
    .AddMinimalApi(
        typeof(Program).Assembly,
        typeof(sample.Application.Register).Assembly,
        typeof(sample.Infrastructure.Register).Assembly
    )
    .AddGlobalExceptionHandling(appName)
    .AddHealthCheck(configuration)
    // Register Infrastructure and Application services
    .AddInfrastructureServices(configuration)
    .AddApplicationServices(configuration)
    // Register OpenAPI/Swagger
    .AddOpenApi();



var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
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

// Map all endpoints from the assembly
app.MapMinimalEndpoints(typeof(Program).Assembly);

await app.RunAsync();
