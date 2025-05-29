using MediatR;
using MinimalAPI.Attributes;
using MinimalAPI.Endpoints;
using sample.Application.Features.Todo.Commands.Update;
using Shared.Responses;

namespace sample.API.Endpoints.Todos.Commands;

[OpenApiSummary("Update an existing todo", 
    Description = "Updates an existing todo item with new information",
    Tags = ["Todo Management", "Commands"])]
[OpenApiParameter("id", typeof(int), Description = "The unique identifier of the todo item to update", Required = true, Location = ParameterLocation.Path)]
[OpenApiResponse(200, ResponseType = typeof(Response<UpdateResponse>), Description = "Todo updated successfully")]
[OpenApiResponse(404, Description = "Todo item not found")]
[OpenApiResponse(400, Description = "Invalid update data")]

public class UpdateTodoEndpoint(IMediator mediator)
    : SingleEndpointBase<UpdateRequest, UpdateResponse>(mediator)
{
    [HttpPut("todos/{id}")]
    public override async Task<Response<UpdateResponse>> HandleAsync(
        UpdateRequest req,
        CancellationToken ct
    ) => await _mediator.Send(req, ct);
}
