using Shared.Entities;

namespace sample.Domain.Entities;

public class Tag : IEntity<int>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
    
    // Navigation properties for many-to-many relationship
    public virtual ICollection<TodoTagItem> TodoTagItems { get; set; } = [];
    public virtual ICollection<TodoItem> TodoItems { get; set; } = [];
}
