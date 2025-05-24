using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using sample.Domain.Entities;

namespace sample.Infrastructure.Persistence.EfCore.Configurations
{
    public class TodoItemConfiguration : IEntityTypeConfiguration<TodoItem>
    {
        public void Configure(EntityTypeBuilder<TodoItem> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Title).IsRequired().HasMaxLength(200);

            builder.Property(t => t.Description).HasMaxLength(1000);

            builder.Property(t => t.IsCompleted).IsRequired();

            builder.Property(t => t.CreatedAt).IsRequired();

            // Configure the relationship from the TodoItem side
            builder.HasOne(t => t.Category)
                .WithMany(c => c.TodoItems)
                .HasForeignKey(t => t.CategoryId)
                .IsRequired(false); // Allow null CategoryId
        }
    }
}
