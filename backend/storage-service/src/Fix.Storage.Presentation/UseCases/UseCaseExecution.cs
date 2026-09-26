using Fix.Storage.Application.Abstractions.Messaging;
using Fix.Storage.Application.Abstractions.Validation;

namespace Fix.Storage.Presentation.UseCases;

/// <summary>
/// Fluxo de entrada da presentation: valida o command/query pela <see cref="IValidationFactory"/> e só então
/// chama o método do service da Application (ex.: <c>validation.RunAsync(command, files.UploadFileAsync, ct)</c>).
/// </summary>
internal static class UseCaseExecution
{
    public static async Task<TResult> RunAsync<TUseCase, TResult>(
        this IValidationFactory validation,
        TUseCase useCase,
        Func<TUseCase, CancellationToken, Task<TResult>> execute,
        CancellationToken cancellationToken)
        where TUseCase : IUseCase
    {
        await validation.ValidateAsync(useCase, cancellationToken);
        return await execute(useCase, cancellationToken);
    }

    public static async Task ExecuteAsync<TUseCase>(
        this IValidationFactory validation,
        TUseCase useCase,
        Func<TUseCase, CancellationToken, Task> execute,
        CancellationToken cancellationToken)
        where TUseCase : IUseCase
    {
        await validation.ValidateAsync(useCase, cancellationToken);
        await execute(useCase, cancellationToken);
    }
}
