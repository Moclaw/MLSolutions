using Microsoft.Extensions.DependencyInjection;
using MinimalAPI.Handlers;
using sample.Application.Features.Todo.Dtos;
using sample.Domain.Constants;
using sample.Domain.Entities;
using Shard.Utils;

namespace sample.Application.Features.Todo.Queries.GetAll;

public class GetAllHandler([FromKeyedServices(ServiceKeys.QueryRepository)] IQueryRepository<TodoItem, int> queryRepository) : IQueryCollectionHandler<GetAllRequest, GetallResponse>
{
    public async Task<Response<GetallResponse>> Handle(GetAllRequest request, CancellationToken cancellationToken)
    {
        // Use builder to apply ordering and paging since repository does not support 'orderBy' parameter directly
        var todoItems = await queryRepository.GetAllAsync(
            t => t.Title.Contains(request.Search ?? string.Empty),
            builder: b =>
            {
               b.OrderByProperty(request.OrderBy ?? nameof(TodoItem.CreatedAt), request.IsAscending);
            },
            paging: new Paging(default, request.PageIndex, request.PageSize),
            enableTracking: false,
            cancellationToken: cancellationToken
        );


        var totalCount = todoItems.Count();

        return ResponseUtils.Success(
            new GetallResponse
            {
                Items = [.. todoItems.Select(TodoItemDto.FromEntity)],
                TotalCount = totalCount,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
            },
            "Todo items retrieved successfully"
        );
    }
}
