using MinimalAPI.Handlers.Command;

namespace sample.Application.Features.Todo.Commands.Delete;

public class Request : ICommand
{
    public int Id { get; set; }
}
