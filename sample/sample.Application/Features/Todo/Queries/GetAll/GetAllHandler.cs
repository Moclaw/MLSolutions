using EfCore.IRepositories;
using Microsoft.Extensions.DependencyInjection;
using MinimalAPI.Handlers;
using sample.Domain.Constants;
using sample.Domain.Entities;
using Shared.Utils;

namespace sample.Application.Features.Todo.Queries.GetAll;

public class GetAllHandler(
    [FromKeyedServices(ServiceKeys.QueryRepository)] IQueryRepository<TodoItem, int> queryRepository
) : IQueryCollectionHandler<GetAllRequest, GetallResponse>
{
    public async Task<Shared.Responses.ResponseCollection<GetallResponse>> Handle(
        GetAllRequest request,
        CancellationToken cancellationToken
    )
    {
        var todoItems = await queryRepository.GetAllAsync<GetallResponse>(
            predicate: t =>
                string.IsNullOrEmpty(request.Search) || t.Title.Contains(request.Search),
            builder: b =>
            {
                b.Include(t => t.Category);
                b.Include(t => t.TodoTagItems).ThenInclude(tt => tt.Tag);
            },
            projector: query =>
                query.Select(item => new GetallResponse
                {
                    Id = item.Id,
                    Title = item.Title,
                    Description = item.Description,
                    IsCompleted = item.IsCompleted,
                    CreatedAt = item.CreatedAt,
                    CompletedAt = item.CompletedAt,
                    CategoryId = item.CategoryId,
                    CategoryName = item.Category!.Name,
                    Tags = item
                        .TodoTagItems.Select(tt => new Dtos.TagDto
                        {
                            Id = tt.Tag.Id,
                            Name = tt.Tag.Name,
                            Color = tt.Tag.Color,
                        })
                        .ToList(),
                }),
            paging: new Pagination(default, request.PageIndex, request.PageSize),
            enableTracking: false,
            cancellationToken: cancellationToken
        );
        return new Shared.Responses.ResponseCollection<GetallResponse>(
            IsSuccess: true,
            Message: "Todo items retrieved successfully.",
            Data: todoItems.Entities,
            StatusCode: 200
        );
    }
}
