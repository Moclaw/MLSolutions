using Host;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var appName = builder.Environment.ApplicationName;
var configuration = builder.Configuration;

// Corrected the chaining to ensure 'AddApplicationLog' is called on 'builder' instead of 'Services'
builder
    .AddApplicationLog(configuration, appName);

builder.Services
    .AddCorsServices(configuration)
    .AddGlobalExceptionHandling(appName)
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


app.MapGet("/", (ILogger<Program> logger) =>
{
    logger.LogInformation("Hello World!");
    return "Hello World!";
});

await app.RunAsync();
