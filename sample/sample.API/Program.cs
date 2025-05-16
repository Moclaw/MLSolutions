using Host;
using Host.Services;
using Serilog;
using sample.Application;
using sample.Application.DependencyInjection;
using Services.Autofac.Extensions;

var builder = WebApplication.CreateBuilder(args);

var appName = builder.Environment.ApplicationName;
var configuration = builder.Configuration;

// Configure Serilog
builder.AddSerilog(configuration, appName);

// Use Autofac as the service provider factory
builder.Host.UseServiceProviderFactory(
    builder.Services.AddAutofacWithApplicationServices(configuration));

// Register other services
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

app.UseRouting();

//configure Health Check
app.UseHealthChecks(configuration);

await app.RunAsync();
