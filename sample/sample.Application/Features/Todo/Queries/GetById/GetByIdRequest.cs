using MinimalAPI.Attributes;
using MinimalAPI.Handlers;

namespace sample.Application.Features.Todo.Queries.GetById;

public class GetByIdRequest : IQueryRequest<GetByIdResponse>
{
    [FromRoute]
    public int Id { get; set; }
}
