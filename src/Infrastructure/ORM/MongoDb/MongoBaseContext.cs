using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace EfCore;

public sealed class MongoBaseContext : DbContext
{
    public IMongoDatabase? MongoDatabase { get; internal set; }

    public IMongoClient? MongoClient { get; internal set; }

    private MongoBaseContext(DbContextOptions options)
        : base(options)
    {
        var optionsExtension = options.Extensions.OfType<MongoOptionsExtension>().FirstOrDefault();
        if (optionsExtension == null)
        {
            return;
        }

        MongoClient = optionsExtension.MongoClient;

        MongoDatabase = optionsExtension.MongoClient?.GetDatabase(optionsExtension.DatabaseName);
    }
}
