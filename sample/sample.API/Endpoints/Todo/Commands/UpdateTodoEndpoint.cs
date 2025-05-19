using MediatR;
using MinimalAPI.Attributes;
using MinimalAPI.Endpoints;
using sample.Application.Features.Todo.Commands.Update;
using Shard.Responses;

namespace sample.API.Endpoints.Todo.Commands;

[Route("api/todos/{id}")]
public class UpdateTodoEndpoint(IMediator mediator) : EndpointBase<UpdateRequest, UpdateResponse>(mediator)
{
    [HttpPut]
    public override async Task<Response<UpdateResponse>> HandleAsync(UpdateRequest req, CancellationToken ct)
    {
        return await _mediator.Send(req, ct);
    }

}
