using EfCore.IRepositories;
using Microsoft.Extensions.DependencyInjection;
using MinimalAPI.Handlers.Command;
using sample.Domain.Constants;
using sample.Domain.Entities;
using Shared.Responses;
using Shared.Utils;

namespace sample.Application.Features.Todo.Commands.Delete;

public class Handler(
    [FromKeyedServices(ServiceKeys.CommandRepository)]
    ICommandRepository commandRepository,
    [FromKeyedServices(ServiceKeys.QueryRepository)]
    IQueryRepository<TodoItem, int> queryRepository
) : ICommandHandler<Request>
{
    public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
    {
        var todoItem = await queryRepository.FirstOrDefaultAsync(
            t => t.Id == request.Id,
            cancellationToken: cancellationToken
        );

        if (todoItem == null)
        {
            return ResponseUtils.Error($"Todo item with ID {request.Id} not found", 404);
        }

        await commandRepository.DeleteAsync(todoItem, cancellationToken);
        await commandRepository.SaveChangesAsync(cancellationToken);

        return ResponseUtils.Success("Todo item deleted successfully");
    }
}
