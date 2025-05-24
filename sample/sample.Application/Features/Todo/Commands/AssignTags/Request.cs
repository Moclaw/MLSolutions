using MinimalAPI.Handlers;
using MinimalAPI.Handlers.Command;

namespace sample.Application.Features.Todo.Commands.AssignTags;
public class Request : ICommand<Response>
{
    public int TodoId { get; set; }
    public List<int> TagIds { get; set; } = new();
}