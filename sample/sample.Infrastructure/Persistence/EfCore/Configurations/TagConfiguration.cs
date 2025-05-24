using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using sample.Domain.Entities;

namespace sample.Infrastructure.Persistence.EfCore.Configurations
{
    public class TagConfiguration : IEntityTypeConfiguration<Tag>
    {
        public void Configure(EntityTypeBuilder<Tag> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(t => t.Color)
                .HasMaxLength(20);

            // Configure many-to-many relationship
            builder.HasMany(t => t.TodoItems)
                .WithMany(i => i.Tags)
                .UsingEntity(
                    "TodoItemTags",
                    l => l.HasOne(typeof(TodoItem)).WithMany().HasForeignKey("TodoItemId"),
                    r => r.HasOne(typeof(Tag)).WithMany().HasForeignKey("TagId"));
        }
    }
}