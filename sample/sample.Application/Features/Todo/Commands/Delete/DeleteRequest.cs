using MinimalAPI.Attributes;
using MinimalAPI.Handlers.Command;

namespace sample.Application.Features.Todo.Commands.Delete;

public class Request : ICommand
{
    [FromRoute]
    public int Id { get; set; }
}
