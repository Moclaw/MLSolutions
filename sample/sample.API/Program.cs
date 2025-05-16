using Host;
using Host.Services;

var builder = WebApplication.CreateBuilder(args);

var appName = builder.Environment.ApplicationName;
var configuration = builder.Configuration;

// Configure Serilog
builder.AddSerilog(configuration, appName);

// Register other services
builder
    .Services.AddCorsServices(configuration)
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
