using MediatR;
using Shard.Responses;

namespace MinimalAPI.Handlers.Command;

public interface ICommand : IRequest<Response>;

public interface ICommand<TResponse> : IRequest<Response<TResponse>>;

public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Response>
    where TCommand : ICommand;

public interface ICommandHandler<in TCommand, TResponse>
    : IRequestHandler<TCommand, Response<TResponse>>
    where TCommand : ICommand<TResponse>;
