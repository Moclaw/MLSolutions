using EfCore.IRepositories;
using Microsoft.Extensions.DependencyInjection;
using MinimalAPI.Handlers.Command;
using sample.Domain.Constants;
using sample.Domain.Entities;
using Shared.Responses;
using Shared.Utils;

namespace sample.Application.Features.Todo.Commands.Update;

public class UpdateHandler(
    [FromKeyedServices(ServiceKeys.CommandRepository)]
    ICommandRepository commandRepository,
    [FromKeyedServices(ServiceKeys.QueryRepository)]
    IQueryRepository<TodoItem, int> queryRepository) : ICommand<UpdateResponse>
{
    public async Task<Response<UpdateResponse>> Handle(
        UpdateRequest request,
        CancellationToken cancellationToken
    )
    {
        var todoItem = await queryRepository
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken: cancellationToken);

        if (todoItem == null)
        {
            return ResponseUtils.Error<UpdateResponse>(203, $"Todo item with ID {request.Id} not found");
        }

        // Update properties
        todoItem.Title = request.Title;
        todoItem.Description = request.Description;

        // Handle completion status change
        if (!todoItem.IsCompleted && request.IsCompleted)
        {
            todoItem.IsCompleted = true;
            todoItem.CompletedAt = DateTime.UtcNow;
        }
        else if (todoItem.IsCompleted && !request.IsCompleted)
        {
            todoItem.IsCompleted = false;
            todoItem.CompletedAt = null;
        }

        await commandRepository.UpdateAsync(todoItem, cancellationToken);
        await commandRepository.SaveChangesAsync(cancellationToken);

        return ResponseUtils.Success(
            new UpdateResponse { Id = todoItem.Id, IsCompleted = todoItem.IsCompleted },
            "Todo item updated successfully"
        );
    }
}
