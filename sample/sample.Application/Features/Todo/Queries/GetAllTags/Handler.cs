using Microsoft.Extensions.DependencyInjection;
using MinimalAPI.Attributes;
using MinimalAPI.Handlers;
using sample.Domain.Constants;
using sample.Domain.Entities;
using Shared.Responses;
using Shared.Utils;

namespace sample.Application.Features.Todo.Queries.GetAllTags;
public class Handler(
    [FromKeyedServices(ServiceKeys.QueryRepository)]
    IQueryRepository<Tag, int> queryRepository
) : IQueryCollectionHandler<Request, Response>
{
    public async Task<ResponseCollection<Response>> Handle(
        Request request,
        CancellationToken cancellationToken
    )
    {
        var tags = await queryRepository.GetAllAsync<Response>(
            predicate: t =>
                string.IsNullOrEmpty(request.Search)
                || t.Name.Contains(request.Search)
                || t.Color!.Contains(request.Search),
            projector: query =>
                query.Select(item => new Response
                {
                    Id = item.Id,
                    Name = item.Name,
                    Color = item.Color
                }),
            paging: new Pagination(default, request.PageIndex, request.PageSize),
            enableTracking: false,
            cancellationToken: cancellationToken
        );

        return new ResponseCollection<Response>(
            IsSuccess: true,
            Message: "Tags retrieved successfully.",
            Data: tags.Entities,
            StatusCode: 200
        );
    }
}