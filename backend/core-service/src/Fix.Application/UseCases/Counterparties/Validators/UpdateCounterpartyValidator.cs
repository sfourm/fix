using Fix.Application.Counterparties.Commands;
using Fix.Domain.Common;
using FluentValidation;

namespace Fix.Application.Counterparties.Validators;

internal sealed class UpdateCounterpartyValidator : AbstractValidator<UpdateCounterpartyCommand>
{
    public UpdateCounterpartyValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Name.MaxLength);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Country).Length(2).When(x => !string.IsNullOrWhiteSpace(x.Country));
        RuleFor(x => x.NotionalLimitUsd).GreaterThanOrEqualTo(0).When(x => x.NotionalLimitUsd is not null);
        RuleFor(x => x.MtmLimitUsd).GreaterThanOrEqualTo(0).When(x => x.MtmLimitUsd is not null);
    }
}
