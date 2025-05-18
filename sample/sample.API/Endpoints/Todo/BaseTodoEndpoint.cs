using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace sample.Api.Endpoints.Todo;

public static class BaseTodoEndpoint
{
    public static IEndpointRouteBuilder MapTodoEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGroup("/api/todos").WithTags("Todos").WithOpenApi();

        return endpoints;
    }
}
