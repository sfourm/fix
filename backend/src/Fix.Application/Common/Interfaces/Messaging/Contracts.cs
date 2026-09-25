namespace Fix.Application.Abstractions.Messaging;

public readonly record struct Unit
{
    public static readonly Unit Value = new();
}

/// <summary>Marcador comum de commands e queries.</summary>
public interface IRequest<out TResult>;

/// <summary>Operação que altera estado.</summary>
public interface ICommand<out TResult> : IRequest<TResult>;

/// <summary>Command sem retorno.</summary>
public interface ICommand : ICommand<Unit>;

/// <summary>Operação somente leitura.</summary>
public interface IQuery<out TResult> : IRequest<TResult>;

public interface ICommandHandler<in TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken);
}

public interface IQueryHandler<in TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken);
}

public delegate Task<TResult> RequestHandlerDelegate<TResult>();

/// <summary>Comportamento transversal executado em volta de todo command/query.</summary>
public interface IPipelineBehavior<in TRequest, TResult>
    where TRequest : notnull
{
    Task<TResult> HandleAsync(TRequest request, RequestHandlerDelegate<TResult> next, CancellationToken cancellationToken);
}

public interface IDispatcher
{
    Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default);

    Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);
}
