using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using sample.Domain.Entities;

namespace sample.Infrastructure.Persistence.EfCore.Configurations
{
    public class TodoTagItemConfiguration : IEntityTypeConfiguration<TodoTagItem>
    {
        public void Configure(EntityTypeBuilder<TodoTagItem> builder)
        {
            builder.HasKey(tt => tt.Id);

            builder.Property(tt => tt.CreatedAt)
                .IsRequired();

            // Configure relationships
            builder.HasOne(tt => tt.TodoItem)
                .WithMany(t => t.TodoTagItems)
                .HasForeignKey(tt => tt.TodoItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(tt => tt.Tag)
                .WithMany(t => t.TodoTagItems)
                .HasForeignKey(tt => tt.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            // Create a unique index on TodoItemId and TagId
            builder.HasIndex(tt => new { tt.TodoItemId, tt.TagId })
                .IsUnique();
        }
    }
}
