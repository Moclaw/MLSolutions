using EfCore.Repositories;
using Microsoft.Extensions.Logging;
using sample.Infrastructure.Persistence.EfCore;

namespace sample.Infrastructure.Repositories;

public class CommandDefaultRepository(ApplicationDbContext dbContext, ILogger<CommandDefaultRepository> logger)
    : CommandRepository(dbContext, logger)
{ }
