using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace MongoDb;

public static partial class Register
{
    /// <summary>
    /// Note: Do not call UseMongoDb. It will be override by this method
    /// </summary>
    /// <typeparam name="TDbContext"></typeparam>
    /// <param name="services"></param>
    /// <param name="connectionString"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IServiceCollection AddMongoDbContext<TDbContext>(
        this IServiceCollection services,
        string connectionString,
        Action<DbContextOptionsBuilder>? options = null
    )
        where TDbContext : DbContext
    {
        var settings = MongoClientSettings.FromConnectionString(connectionString);
        var client = new MongoClient(settings);
        var url = new MongoUrl(connectionString);

        services.AddSingleton<IMongoClient>(client);

        services.AddDbContext<TDbContext>(opts =>
        {
            options?.Invoke(opts);

            opts.UseMongoDB(client, url.DatabaseName);
        });

        return services;
    }
}
