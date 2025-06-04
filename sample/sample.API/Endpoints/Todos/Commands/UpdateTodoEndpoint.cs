using MediatR;
using MinimalAPI.Attributes;
using MinimalAPI.Endpoints;
using sample.Application.Features.Todo.Commands.Update;
using Shared.Responses;

namespace sample.API.Endpoints.Todos.Commands;

[OpenApiSummary("Update an existing todo", 
    Description = "Updates an existing todo item with new information")]
[OpenApiResponse(200, ResponseType = typeof(Response<UpdateResponse>), Description = "Todo updated successfully")]
[OpenApiResponse(404, Description = "Todo item not found")]
[OpenApiResponse(400, Description = "Invalid update data")]
[ApiVersion("1.0")]

public class UpdateTodoEndpoint(IMediator mediator)
    : SingleEndpointBase<UpdateRequest, UpdateResponse>(mediator)
{
    [HttpPut("todos/{id}")]
    public override async Task<Response<UpdateResponse>> HandleAsync(
        UpdateRequest req,
        CancellationToken ct
    ) => await _mediator.Send(req, ct);
}
