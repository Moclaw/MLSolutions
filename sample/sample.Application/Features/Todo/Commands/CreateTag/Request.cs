using MinimalAPI.Handlers;
using MinimalAPI.Handlers.Command;

namespace sample.Application.Features.Todo.Commands.CreateTag;
public class Request : ICommand<Response>
{
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
}