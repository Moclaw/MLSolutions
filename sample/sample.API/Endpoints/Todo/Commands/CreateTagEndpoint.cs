using MediatR;
using MinimalAPI.Attributes;
using MinimalAPI.Endpoints;
using sample.Application.Features.Todo.Commands.CreateTag;
using Response = sample.Application.Features.Todo.Commands.CreateTag.Response;

namespace sample.API.Endpoints.Todo.Commands;
public class CreateTagEndpoint(IMediator mediator)
    : SingleEndpointBase<Request, Response>(mediator)
{
    [HttpPost("api/tags")]
    public override async Task<Shared.Responses.Response<Response>> HandleAsync(
        Request req,
        CancellationToken ct
    )
    {
        return await _mediator.Send(req, ct);
    }
}