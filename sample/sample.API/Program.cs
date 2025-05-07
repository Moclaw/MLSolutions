using Host;
using Host.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var appName = builder.Environment.ApplicationName;
var configuration = builder.Configuration;

// Corrected the chaining to ensure 'AddApplicationLog' is called on 'builder' instead of 'Services'
builder
    .AddSerilog(configuration, appName);

builder.Services
    .AddCorsServices(configuration)
    .AddGlobalExceptionHandling(appName)
    .AddHealthCheck(configuration)
    .AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

//configure CORS
app.UseCorsServices(configuration);

//configure Global Exception Handling
app.UseGlobalExceptionHandling();

//configure ARM Elastic
app.UseElasticApm(configuration);

app.MapGet("/", (ILogger<Program> logger) =>
{
    logger.LogInformation("Resource not found.");
    return Results.NotFound("Resource not found.");
});

await app.RunAsync();
