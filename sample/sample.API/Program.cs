using Host;
using Host.Services;
using Microsoft.EntityFrameworkCore;
using MinimalAPI;
using sample.Application;
using sample.Infrastructure;
using sample.Infrastructure.Persistence.EfCore;

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

using (var scope = app.Services.CreateScope())
{
    // Initialize the database
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    // Ensure the database is created and apply migrations
    await db.Database.EnsureCreatedAsync();
    await db.Database.MigrateAsync();
    
    // Initialize S3 bucket (useful for LocalStack)
    var s3Service = scope.ServiceProvider.GetService<Services.AWS.S3.Interfaces.IS3Service>();
    if (s3Service != null)
    {
        await s3Service.EnsureBucketExistsAsync();
    }
    
    // Seed data if the database is empty
    if (!db.Set<sample.Domain.Entities.TodoItem>().Any())
    {
        // Create some tags
        var tags = new List<sample.Domain.Entities.Tag>
        {
            new() { Name = "Important", Color = "#FF0000" },
            new() { Name = "Personal", Color = "#0000FF" },
            new() { Name = "Work", Color = "#008000" },
            new() { Name = "Urgent", Color = "#FF4500" },
            new() { Name = "Low Priority", Color = "#FFFF00" },
            new() { Name = "Health", Color = "#FF69B4" },
            new() { Name = "Finance", Color = "#32CD32" },
            new() { Name = "Family", Color = "#9932CC" },
        };

        await db.Set<sample.Domain.Entities.Tag>().AddRangeAsync(tags);

        // Create some todo categories
        var categories = new List<sample.Domain.Entities.TodoCategory>
        {
            new() { Name = "Home" },
            new() { Name = "Work" },
            new() { Name = "Study" },
            new() { Name = "Health" },
            new() { Name = "Financial" },
            new() { Name = "Personal Development" },
        };

        await db.Set<sample.Domain.Entities.TodoCategory>().AddRangeAsync(categories);

        // Create some todo items with categories and tags
        var todoItems = new List<sample.Domain.Entities.TodoItem>
        {
            new()
            {
                Title = "Buy groceries",
                Description = "Milk, bread, eggs",
                IsCompleted = false,
                Category = categories[0],
                Tags = new List<sample.Domain.Entities.Tag> { tags[0], tags[1] },
            },
            new()
            {
                Title = "Finish project",
                Description = "Complete the pending tasks",
                IsCompleted = false,
                Category = categories[1],
                Tags = new List<sample.Domain.Entities.Tag> { tags[0], tags[2], tags[3] },
            },
            new()
            {
                Title = "Read book",
                Description = "Chapter 5 and 6",
                IsCompleted = true,
                CompletedAt = DateTime.UtcNow.AddDays(-1),
                Category = categories[2],
            },
            new()
            {
                Title = "Go for a run",
                Description = "30 minutes morning run",
                IsCompleted = true,
                CompletedAt = DateTime.UtcNow.AddDays(-2),
                Category = categories[3],
                Tags = new List<sample.Domain.Entities.Tag> { tags[5] },
            },
            new()
            {
                Title = "Pay electricity bill",
                Description = "Due by end of month",
                IsCompleted = false,
                Category = categories[4],
                Tags = new List<sample.Domain.Entities.Tag> { tags[6], tags[4] },
            },
            new()
            {
                Title = "Learn new programming language",
                Description = "Complete online course",
                IsCompleted = false,
                Category = categories[5],
                Tags = new List<sample.Domain.Entities.Tag> { tags[2], tags[1] },
            },
            new()
            {
                Title = "Family dinner",
                Description = "Prepare dinner for Sunday",
                IsCompleted = false,
                Category = categories[0],
                Tags = new List<sample.Domain.Entities.Tag> { tags[7], tags[1] },
            },
            new()
            {
                Title = "Dentist appointment",
                Description = "Annual checkup",
                IsCompleted = false,
                Category = categories[3],
                Tags = new List<sample.Domain.Entities.Tag> { tags[3], tags[5] },
            },
        };

        await db.Set<sample.Domain.Entities.TodoItem>().AddRangeAsync(todoItems);

        await db.SaveChangesAsync();
    }
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
