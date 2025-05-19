using Microsoft.Extensions.DependencyInjection;
using MinimalAPI.Handlers;
using sample.Domain.Constants;
using sample.Domain.Entities;
using Shard.Responses;
using Shard.Utils;

namespace sample.Application.Features.Todo.Queries.GetById;

public class Handler([FromKeyedServices(ServiceKeys.QueryRepository)] IQueryRepository<TodoItem, int> queryRepository)
    : IQueryHandler<Request, Response>
{
    public async Task<Response<Response>> Handle(
        Request request,
        CancellationToken cancellationToken
    )
    {
        var todoItem = await queryRepository.FirstOrDefaultAsync(
            t => t.Id == request.Id,
            cancellationToken: cancellationToken
        );

        if (todoItem == null)
        {
            return ResponseUtils.Error<Response>(203, $"Todo item with ID {request.Id} not found");
        }

        return ResponseUtils.Success(
            Response.FromEntity(todoItem),
            "Todo item retrieved successfully"
        );
    }
}
