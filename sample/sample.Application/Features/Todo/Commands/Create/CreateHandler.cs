using EfCore.IRepositories;
using Microsoft.Extensions.DependencyInjection;
using MinimalAPI.Handlers.Command;
using sample.Domain.Constants;
using sample.Domain.Entities;
using Shard.Utils;

namespace sample.Application.Features.Todo.Commands.Create;

public class CreateHandler(
    [FromKeyedServices(ServiceKeys.CommandRepository)]
    ICommandRepository commandRepository) : ICommandHandler<CreateRequest, Response>
{
    public async Task<Response<Response>> Handle(CreateRequest request, CancellationToken cancellationToken)
    {
        var todoItem = new TodoItem
        {
            Title = request.Title,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow,
            CompletedAt = null,
            IsCompleted = false
        };
        await commandRepository.AddAsync(todoItem, cancellationToken);
        await commandRepository.SaveChangesAsync(cancellationToken);
        return ResponseUtils.Success(
            new Response { Id = todoItem.Id },
            "Todo item created successfully"
        );
    }
}
