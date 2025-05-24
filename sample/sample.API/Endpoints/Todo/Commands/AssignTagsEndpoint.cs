using MediatR;
using MinimalAPI.Attributes;
using MinimalAPI.Endpoints;
using sample.Application.Features.Todo.Commands.AssignTags;
using Response = sample.Application.Features.Todo.Commands.AssignTags.Response;

namespace sample.API.Endpoints.Todo.Commands;
public class AssignTagsEndpoint(IMediator mediator)
    : SingleEndpointBase<Request, Response>(mediator)
{
    [HttpPost("api/todos/{todoId}/tags")]
    public override async Task<Shared.Responses.Response<Application.Features.Todo.Commands.AssignTags.Response>>
        HandleAsync(
            Request req,
            CancellationToken ct
        )
    {
        return await _mediator.Send(req, ct);
    }
}