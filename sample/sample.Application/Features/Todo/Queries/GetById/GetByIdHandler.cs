using EfCore.IRepositories;
using Microsoft.Extensions.DependencyInjection;
using MinimalAPI.Handlers;
using sample.Domain.Constants;
using sample.Domain.Entities;
using Shared.Responses;
using Shared.Utils;

namespace sample.Application.Features.Todo.Queries.GetById;

public class GetByIdHandler([FromKeyedServices(ServiceKeys.QueryRepository)] IQueryRepository<TodoItem, int> queryRepository)
    : IQueryHandler<GetByIdRequest, GetByIdResponse>
{
    public async Task<Response<GetByIdResponse>> Handle(
        GetByIdRequest request,
        CancellationToken cancellationToken
    )
    {
        var todoItem = await queryRepository.FirstOrDefaultAsync(
            t => t.Id == request.Id,
            builder: b => b
                .Include(t => t.Category)
                .Include(t => t.Tags),
            cancellationToken: cancellationToken
        );

        if (todoItem == null)
        {
            return ResponseUtils.Error<GetByIdResponse>(203, $"Todo item with ID {request.Id} not found");
        }

        return ResponseUtils.Success(
            GetByIdResponse.FromEntity(todoItem),
            "Todo item retrieved successfully"
        );
    }
}
