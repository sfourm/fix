using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace Fix.Application.Abstractions.Messaging;

internal sealed class Dispatcher(IServiceProvider serviceProvider) : IDispatcher
{
    private static readonly ConcurrentDictionary<Type, object> Wrappers = new();

    public Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default)
    {
        var wrapper = (RequestWrapper<TResult>)Wrappers.GetOrAdd(
            command.GetType(),
            type => Activator.CreateInstance(typeof(CommandWrapper<,>).MakeGenericType(type, typeof(TResult)))!);

        return wrapper.HandleAsync(command, serviceProvider, cancellationToken);
    }

    public Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
    {
        var wrapper = (RequestWrapper<TResult>)Wrappers.GetOrAdd(
            query.GetType(),
            type => Activator.CreateInstance(typeof(QueryWrapper<,>).MakeGenericType(type, typeof(TResult)))!);

        return wrapper.HandleAsync(query, serviceProvider, cancellationToken);
    }

    private abstract class RequestWrapper<TResult>
    {
        public abstract Task<TResult> HandleAsync(object request, IServiceProvider provider, CancellationToken cancellationToken);

        protected static Task<TResult> RunPipeline<TRequest>(
            TRequest request,
            Func<Task<TResult>> handler,
            IServiceProvider provider,
            CancellationToken cancellationToken)
            where TRequest : notnull
        {
            RequestHandlerDelegate<TResult> next = () => handler();

            // O primeiro behavior registrado é o mais externo.
            foreach (var behavior in provider.GetServices<IPipelineBehavior<TRequest, TResult>>().Reverse())
            {
                var inner = next;
                next = () => behavior.HandleAsync(request, inner, cancellationToken);
            }

            return next();
        }
    }

    private sealed class CommandWrapper<TCommand, TResult> : RequestWrapper<TResult>
        where TCommand : ICommand<TResult>
    {
        public override Task<TResult> HandleAsync(object request, IServiceProvider provider, CancellationToken cancellationToken)
        {
            var command = (TCommand)request;
            var handler = provider.GetRequiredService<ICommandHandler<TCommand, TResult>>();
            return RunPipeline(command, () => handler.HandleAsync(command, cancellationToken), provider, cancellationToken);
        }
    }

    private sealed class QueryWrapper<TQuery, TResult> : RequestWrapper<TResult>
        where TQuery : IQuery<TResult>
    {
        public override Task<TResult> HandleAsync(object request, IServiceProvider provider, CancellationToken cancellationToken)
        {
            var query = (TQuery)request;
            var handler = provider.GetRequiredService<IQueryHandler<TQuery, TResult>>();
            return RunPipeline(query, () => handler.HandleAsync(query, cancellationToken), provider, cancellationToken);
        }
    }
}
