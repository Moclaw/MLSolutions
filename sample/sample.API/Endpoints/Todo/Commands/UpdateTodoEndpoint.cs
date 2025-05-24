using MediatR;
using MinimalAPI.Attributes;
using MinimalAPI.Endpoints;
using sample.Application.Features.Todo.Commands.Update;
using Shared.Responses;

namespace sample.API.Endpoints.Todo.Commands;

public class UpdateTodoEndpoint(IMediator mediator) : EndpointBase<UpdateRequest, UpdateResponse>(mediator)
{
    [HttpPut("api/todos/{id}")]
    public override async Task<Response<UpdateResponse>> HandleAsync(UpdateRequest req, CancellationToken ct)
    {
        return await _mediator.Send(req, ct);
    }
}
