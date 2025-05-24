using Shared.Entities;

namespace sample.Domain.Entities;

public class TodoCategory : IEntity<int>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Navigation property for the relationship
    public virtual ICollection<TodoItem> TodoItems { get; set; } = new List<TodoItem>();
}
