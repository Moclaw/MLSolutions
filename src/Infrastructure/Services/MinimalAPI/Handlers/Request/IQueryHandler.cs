using MediatR;
using Shared.Responses;

namespace MinimalAPI.Handlers;
public interface IQueryRequest : IRequest<Response>;

public interface IQueryHandler<in TQueryRequest> : IRequestHandler<TQueryRequest, Response>
    where TQueryRequest : IQueryRequest;

public interface IQueryRequest<TResponse> : IRequest<Response<TResponse>>;

public interface IQueryHandler<in TQueryRequest, TResponse> : IRequestHandler<TQueryRequest, Response<TResponse>>
    where TQueryRequest : IQueryRequest<TResponse>;

public interface IQueryCollectionRequest<TResponse> : IRequest<Response<TResponse>>
{
    public string? Search { get; set; }

    public int PageIndex { get; set; }

    public int PageSize { get; set; }

    public string OrderBy { get; set; }

    public bool IsAscending { get; set; }
};

public interface IQueryCollectionHandler<in TQueryRequest, TResponse> : IRequestHandler<TQueryRequest, Response<TResponse>>
    where TQueryRequest : IQueryCollectionRequest<TResponse>;