using Fix.Application.Mandates.Commands;
using Fix.Domain.Common;
using FluentValidation;

namespace Fix.Application.Mandates.Validators;

/// <summary>Regras de formato dos termos; as regras de negócio (enquadramento, fator do eixo) ficam no domínio.</summary>
internal sealed class MandateTermsValidator : AbstractValidator<MandateTermsInput>
{
    public MandateTermsValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(Title.MaxLength);
        RuleFor(x => x.Criteria).MaximumLength(DomainGuard.DescriptionMaxLength);
        RuleFor(x => x.Commodity).IsInEnum().When(x => x.Commodity is not null);
        RuleFor(x => x.Quantity).GreaterThan(0).When(x => x.Quantity is not null);
        RuleFor(x => x.QuantityUnit).NotNull().When(x => x.Quantity is not null)
            .WithMessage("Informe a unidade da quantidade.");
        RuleFor(x => x.WindowEnd).GreaterThanOrEqualTo(x => x.WindowStart)
            .When(x => x.WindowStart is not null && x.WindowEnd is not null);
    }
}
