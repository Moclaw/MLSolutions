using EfCore.Repositories;
using Microsoft.Extensions.Logging;
using sample.Infrastructure.Persistence;

namespace sample.Infrastructure.Repositories;

public class CommandDefaultRepository(ApplicationDbContext dbContext, ILogger<CommandDefaultRepository> logger)
    : CommadRepository(dbContext, logger) { }
