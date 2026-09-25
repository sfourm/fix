using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Fix.Application.Abstractions.Messaging;

namespace Fix.Application.Authorization;

/// <summary>
/// Proxy registrado no lugar de cada interface de service (IPolicyService, IMandateService...): todo método que
/// recebe um <see cref="IUseCase"/> passa antes pelo <see cref="UseCaseGuard"/>. Assim nenhum caso de uso novo
/// fica sem contexto/autorização, e os services seguem só com a regra de negócio.
/// </summary>
public class UseCaseGuardProxy<TService> : DispatchProxy
    where TService : class
{
    private static readonly MethodInfo RunWithResultDefinition =
        typeof(UseCaseGuardProxy<TService>).GetMethod(nameof(RunWithResultAsync), BindingFlags.NonPublic | BindingFlags.Instance)!;

    private static readonly ConcurrentDictionary<Type, MethodInfo> RunWithResult = new();

    private TService target = null!;
    private UseCaseGuard guard = null!;

    internal static TService Create(TService target, UseCaseGuard guard)
    {
        var proxy = Create<TService, UseCaseGuardProxy<TService>>();
        var instance = (UseCaseGuardProxy<TService>)(object)proxy;
        instance.target = target;
        instance.guard = guard;
        return proxy;
    }

    protected override object? Invoke(MethodInfo? method, object?[]? args)
    {
        ArgumentNullException.ThrowIfNull(method);
        args ??= [];

        var useCase = args.OfType<IUseCase>().FirstOrDefault();
        if (useCase is null)
        {
            return Call(method, args);
        }

        var cancellationToken = args.OfType<CancellationToken>().FirstOrDefault();
        if (method.ReturnType == typeof(Task))
        {
            return RunAsync(method, args, useCase, cancellationToken);
        }

        if (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var run = RunWithResult.GetOrAdd(method.ReturnType.GetGenericArguments()[0], t => RunWithResultDefinition.MakeGenericMethod(t));
            return run.Invoke(this, [method, args, useCase, cancellationToken]);
        }

        throw new InvalidOperationException($"{typeof(TService).Name}.{method.Name} recebe um caso de uso e precisa ser assíncrono.");
    }

    private async Task RunAsync(MethodInfo method, object?[] args, IUseCase useCase, CancellationToken cancellationToken)
    {
        await guard.EnterAsync(useCase, cancellationToken);
        await (Task)Call(method, args)!;
    }

    private async Task<TResult> RunWithResultAsync<TResult>(MethodInfo method, object?[] args, IUseCase useCase, CancellationToken cancellationToken)
    {
        await guard.EnterAsync(useCase, cancellationToken);
        return await (Task<TResult>)Call(method, args)!;
    }

    /// <summary>Chama o service real sem embrulhar exceções síncronas em TargetInvocationException.</summary>
    private object? Call(MethodInfo method, object?[] args)
    {
        try
        {
            return method.Invoke(target, args);
        }
        catch (TargetInvocationException exception) when (exception.InnerException is not null)
        {
            ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
            throw;
        }
    }
}
