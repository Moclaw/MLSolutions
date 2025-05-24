using EfCore.IRepositories;
using Microsoft.Extensions.DependencyInjection;
using MinimalAPI.Attributes;
using MinimalAPI.Handlers;
using MinimalAPI.Handlers.Command;
using sample.Application.Features.Todo.Dtos;
using sample.Domain.Constants;
using sample.Domain.Entities;
using Shared.Responses;
using Shared.Utils;

namespace sample.Application.Features.Todo.Commands.CreateTag;
public class Handler(
    [FromKeyedServices(ServiceKeys.CommandRepository)]
    ICommandRepository commandRepository
) : ICommandHandler<Request, Response>
{
    public async Task<Response<Response>> Handle(
        Request request,
        CancellationToken cancellationToken
    )
    {
        var tag = new Tag
        {
            Name = request.Name,
            Color = request.Color
        };

        await commandRepository.AddAsync(tag, cancellationToken);
        await commandRepository.SaveChangesAsync(cancellationToken);

        var tagDto = new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            Color = tag.Color
        };

        return ResponseUtils.Success(
            Response.FromTagDto(tagDto),
            "Tag created successfully"
        );
    }
}