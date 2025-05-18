using MediatR;

namespace sample.Application.Features.Todo.Commands.Create;

public class Request : IRequest<Response>
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
}