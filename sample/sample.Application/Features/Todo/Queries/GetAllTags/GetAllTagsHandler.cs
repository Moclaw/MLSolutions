using EfCore.IRepositories;
using Microsoft.Extensions.DependencyInjection;
using MinimalAPI.Attributes;
using MinimalAPI.Handlers;
using sample.Domain.Constants;
using sample.Domain.Entities;
using Shared.Responses;
using Shared.Utils;

namespace sample.Application.Features.Todo.Queries.GetAllTags;

public class GetAllTagsHandler(
    [FromKeyedServices(ServiceKeys.QueryRepository)]
    IQueryRepository<Tag, int> queryRepository
) : IQueryCollectionHandler<GetAllTagsRequest, GetAllTagsResponse>
{
    public async Task<ResponseCollection<GetAllTagsResponse>> Handle(
        GetAllTagsRequest request,
        CancellationToken cancellationToken
    )
    {
        var tags = await queryRepository.GetAllAsync<GetAllTagsResponse>(
            predicate: t =>
                string.IsNullOrEmpty(request.Search)
                || t.Name.Contains(request.Search)
                || (t.Color != null && t.Color.Contains(request.Search)),
            projector: query =>
                query.Select(item => new GetAllTagsResponse
                {
                    Id = item.Id,
                    Name = item.Name,
                    Color = item.Color
                }),
            paging: new Pagination(default, request.PageIndex, request.PageSize),
            enableTracking: false,
            cancellationToken: cancellationToken
        );

        return new ResponseCollection<GetAllTagsResponse>(
            IsSuccess: true,
            Message: "Tags retrieved successfully.",
            Data: tags.Entities,
            StatusCode: 200
        );
    }
}