using MinimalAPI.Handlers.Command;

namespace sample.Application.Features.Todo.Commands.Update;

public class UpdateRequest : ICommand<UpdateResponse>
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
}
