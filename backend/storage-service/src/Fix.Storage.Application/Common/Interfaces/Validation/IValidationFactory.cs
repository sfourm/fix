using Fix.Storage.Application.Abstractions.Messaging;

namespace Fix.Storage.Application.Abstractions.Validation;

/// <summary>
/// Valida a entrada de qualquer caso de uso sem conhecer o tipo concreto: localiza os validators registrados para o
/// command/query e lança <see cref="FluentValidation.ValidationException"/> com as falhas. A presentation chama a
/// factory antes de acionar o service da Application.
/// </summary>
public interface IValidationFactory
{
    Task ValidateAsync<TUseCase>(TUseCase useCase, CancellationToken cancellationToken = default)
        where TUseCase : IUseCase;
}
