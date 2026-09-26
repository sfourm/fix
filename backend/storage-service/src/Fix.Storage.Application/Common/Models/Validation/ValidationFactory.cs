using Fix.Storage.Application.Abstractions.Messaging;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;

namespace Fix.Storage.Application.Abstractions.Validation;

internal sealed class ValidationFactory(IServiceProvider serviceProvider) : IValidationFactory
{
    public async Task ValidateAsync<TUseCase>(TUseCase useCase, CancellationToken cancellationToken = default)
        where TUseCase : IUseCase
    {
        ArgumentNullException.ThrowIfNull(useCase);

        var failures = new List<ValidationFailure>();
        foreach (var validator in serviceProvider.GetServices<IValidator<TUseCase>>())
        {
            var result = await validator.ValidateAsync(useCase, cancellationToken);
            failures.AddRange(result.Errors);
        }

        if (failures.Count > 0)
        {
            throw new ValidationException(failures);
        }
    }
}
