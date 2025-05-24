using Microsoft.Extensions.DependencyInjection;
using MinimalAPI.Attributes;
using MinimalAPI.Handlers;
using sample.Application.Features.Todo.Dtos;
using sample.Domain.Constants;
using sample.Domain.Entities;
using Shared.Utils;

namespace sample.Application.Features.Todo.Queries.GetAll;

public class GetAllHandler(
    [FromKeyedServices(ServiceKeys.QueryRepository)] IQueryRepository<TodoItem, int> queryRepository
) : IQueryCollectionHandler<GetAllRequest, GetallResponse>
{
    public async Task<ResponseCollection<GetallResponse>> Handle(
        GetAllRequest request,
        CancellationToken cancellationToken
    )
    {
        var todoItems = await queryRepository.GetAllAsync<GetallResponse>(
            predicate: t =>
                string.IsNullOrEmpty(request.Search) || t.Title.Contains(request.Search),
            builder: b =>
            {
                b.OrderByProperty(
                    request.OrderBy ?? nameof(TodoItem.CreatedAt),
                    request.IsAscending
                );
            },
            projector: query =>
                query.Select(item => new GetallResponse
                {
                    Id = item.Id,
                    Title = item.Title,
                    Description = item.Description,
                    IsCompleted = item.IsCompleted,
                    CreatedAt = item.CreatedAt,
                }),
            paging: new Pagination(default, request.PageIndex, request.PageSize),
            enableTracking: false,
            cancellationToken: cancellationToken
        );
        return new ResponseCollection<GetallResponse>(
            IsSuccess: true,
            Message: "Todo items retrieved successfully.",
            Data: todoItems.Entities,
            StatusCode: 200
        );
    }
}
