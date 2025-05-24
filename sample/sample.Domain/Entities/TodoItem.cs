using Shared.Entities;

namespace sample.Domain.Entities;

public class TodoItem : IEntity<int>
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    // Foreign key for one-to-many relationship
    public int? CategoryId { get; set; }

    // Navigation property for one-to-many relationship
    public virtual TodoCategory? Category { get; set; }

    // Navigation properties for many-to-many relationship
    public virtual ICollection<TodoTagItem> TodoTagItems { get; set; } = new List<TodoTagItem>();
    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
