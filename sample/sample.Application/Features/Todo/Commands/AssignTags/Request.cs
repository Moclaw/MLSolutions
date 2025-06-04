using MinimalAPI.Handlers;
using MinimalAPI.Handlers.Command;
using MinimalAPI.Attributes;

namespace sample.Application.Features.Todo.Commands.AssignTags;
public class Request : ICommand<Response>
{
    [FromRoute(Name = "todoId")]
    public int TodoId { get; set; }
    
    [FromBody]
    public List<int> TagIds { get; set; } = new();
}