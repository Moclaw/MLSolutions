using System.Runtime.CompilerServices;
using EfCore.IRepositories;
using MinimalAPI.Handlers;
using MinimalAPI.Handlers.Command;
using sample.Domain.Entities;
using Shard.Responses;
using Shard.Utils;

namespace sample.Application.Features.Todo.Commands.Create;

public class Handler(ICommandRepository commandRepository) : ICommand<TodoResponse>
{
    public async Task<Response<TodoResponse>> Handle(
        Request request,
        CancellationToken cancellationToken
    )
    {
        var todo = new TodoItem
        {
            Title = request.Title,
            CreatedAt = DateTime.UtcNow,
            IsCompleted = false,
        };

        await commandRepository.AddAsync(todo, cancellationToken);
        await commandRepository.SaveChangesAsync(cancellationToken);

        return (Response<TodoResponse>)
            ResponseUtils.Success(
                new TodoResponse { Id = todo.Id },
                "Todo item created successfully"
            );
    }
}
