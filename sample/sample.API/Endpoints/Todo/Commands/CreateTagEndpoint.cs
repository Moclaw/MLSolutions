using MediatR;
using MinimalAPI.Attributes;
using MinimalAPI.Endpoints;
using sample.Application.Features.Todo.Commands.CreateTag;
using Response = sample.Application.Features.Todo.Commands.CreateTag.Response;

namespace sample.API.Endpoints.Todo.Commands;

[OpenApiSummary("Create a new tag", 
    Description = "Creates a new tag that can be assigned to todo items",
    Tags = ["Tag Management", "Commands"])]
[OpenApiResponse(201, ResponseType = typeof(Response), Description = "Tag created successfully")]
[OpenApiResponse(400, Description = "Invalid tag data")]
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