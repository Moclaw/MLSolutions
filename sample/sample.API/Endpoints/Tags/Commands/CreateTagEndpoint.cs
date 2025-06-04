using MediatR;
using MinimalAPI.Attributes;
using MinimalAPI.Endpoints;
using sample.Application.Features.Todo.Commands.CreateTag;
using Response = sample.Application.Features.Todo.Commands.CreateTag.Response;

namespace sample.API.Endpoints.Tags.Commands;

[OpenApiSummary("Create a new tag", 
    Description = "Creates a new tag that can be assigned to todo items")]
[OpenApiResponse(201, ResponseType = typeof(Response), Description = "Tag created successfully")]
[OpenApiResponse(400, Description = "Invalid tag data")]
[ApiVersion("1.0")]
public class CreateTagEndpoint(IMediator mediator)
    : SingleEndpointBase<Request, Response>(mediator)
{
    [HttpPost("tags")]
    public override async Task<Response<Response>> HandleAsync(
        Request req,
        CancellationToken ct
    ) => await _mediator.Send(req, ct);
}