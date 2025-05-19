using MediatR;
using MinimalAPI.Handlers.Command;

namespace sample.Application.Features.Todo.Commands.Create;

public class CreateRequest : ICommand<Response>
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
}