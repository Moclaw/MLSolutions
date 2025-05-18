using System.Reflection;
using EfCore;
using Microsoft.EntityFrameworkCore;
using Core;

namespace sample.Infrastructure.Persistence
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : BaseDbContext(options)
    {
        protected override Assembly ExecutingAssembly => typeof(ApplicationDbContext).Assembly;

        protected override Func<Type, bool> RegisterConfigurationsPredicate => 
            t => t.IsClass && typeof(IEntityTypeConfiguration<>).IsAssignableFrom(t);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Apply configurations from the specified namespace
            
            var configurations = AssemblyScanner.FindTypesInNamespace(
                Assembly.GetExecutingAssembly(),
                t => t.IsClass && typeof(IEntityTypeConfiguration<>).IsAssignableFrom(t)
            );
        }
    }
}
