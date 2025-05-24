using Shared.Entities;

namespace sample.Domain.Entities;

public class TodoTagItem : IEntity<int>
{
    public int Id { get; set; }
    public int TodoItemId { get; set; }
    public int TagId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual TodoItem TodoItem { get; set; } = null!;
    public virtual Tag Tag { get; set; } = null!;
}
