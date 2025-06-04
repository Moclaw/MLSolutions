using MediatR;
using MinimalAPI.Attributes;
using MinimalAPI.Endpoints;
using sample.Application.Features.Todo.Commands.AssignTags;
using Response = sample.Application.Features.Todo.Commands.AssignTags.Response;

namespace sample.API.Endpoints.Tags.Commands;

[OpenApiSummary("Assign tags to todo", 
    Description = "Assigns multiple tags to a specific todo item")]
[OpenApiResponse(200, ResponseType = typeof(Response), Description = "Tags assigned successfully")]
[OpenApiResponse(404, Description = "Todo item or tags not found")]
[ApiVersion("1.0")]
public class AssignTagsEndpoint(IMediator mediator)
    : SingleEndpointBase<Request, Response>(mediator)
{
    [HttpPost("todos/{todoId}/tags")]
    public override async Task<Response<Response>>
        HandleAsync(
            Request req,
            CancellationToken ct
        ) => await _mediator.Send(req, ct);
}