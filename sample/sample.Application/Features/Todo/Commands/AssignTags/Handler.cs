using EfCore.IRepositories;
using Microsoft.Extensions.DependencyInjection;
using MinimalAPI.Attributes;
using MinimalAPI.Handlers;
using MinimalAPI.Handlers.Command;
using sample.Domain.Constants;
using sample.Domain.Entities;
using Shared.Responses;
using Shared.Utils;

namespace sample.Application.Features.Todo.Commands.AssignTags;
public class Handler(
    [FromKeyedServices(ServiceKeys.QueryRepository)]
    IQueryRepository<TodoItem, int> queryRepository,
    [FromKeyedServices(ServiceKeys.CommandRepository)]
    ICommandRepository commandRepository
) : ICommandHandler<Request, Response>
{
    public async Task<Response<Response>> Handle(
        Request request,
        CancellationToken cancellationToken
    )
    {
        // Get the todo item with its existing tags
        var todoItem = await queryRepository.FirstOrDefaultAsync(
            t => t.Id == request.TodoId,
            builder: b => b.Include(t => t.Tags),
            cancellationToken: cancellationToken
        );

        if (todoItem == null)
        {
            return ResponseUtils.Error<Response>(404, $"Todo item with ID {request.TodoId} not found");
        }

        // Get all requested tags
        var tags = await queryRepository.GetAllAsync<Tag>(
            predicate: t => request.TagIds.Contains(t.Id),
            cancellationToken: cancellationToken
        );

        var foundTagIds = tags.Entities.Select(t => t.Id).ToList();
        var notFoundTagIds = request.TagIds.Except(foundTagIds).ToList();

        if (notFoundTagIds.Any())
        {
            return ResponseUtils.Error<Response>(
                404,
                $"Tags with IDs {string.Join(", ", notFoundTagIds)} not found"
            );
        }

        // Clear existing tags and add new ones
        todoItem.Tags.Clear();
        foreach (var tag in tags.Entities)
        {
            todoItem.Tags.Add(tag);
        }

        await commandRepository.UpdateAsync(todoItem, cancellationToken);
        await commandRepository.SaveChangesAsync(cancellationToken);

        return ResponseUtils.Success(
            new Response { TodoId = todoItem.Id, AssignedTagIds = foundTagIds },
            "Tags assigned successfully"
        );
    }
}