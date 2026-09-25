using Fix.Application.Orders.Commands;
using Fix.Domain.Common;
using Fix.Domain.AggregateRoots.Orders;
using FluentValidation;

namespace Fix.Application.Orders.Validators;

/// <summary>Regras de formato da boleta; saldo, homologação e compatibilidade com o mandato ficam no domínio.</summary>
internal sealed class OrderTermsValidator : AbstractValidator<OrderTermsInput>
{
    public OrderTermsValidator()
    {
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Direction).IsInEnum();
        RuleFor(x => x.Tenor).NotEmpty().MaximumLength(10);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.Lots).NotNull().GreaterThan(0).When(x => x.Type != OrderType.Ndf)
            .WithMessage("Informe a quantidade de lotes.");
        RuleFor(x => x.NotionalUsd).NotNull().GreaterThan(0).When(x => x.Type == OrderType.Ndf)
            .WithMessage("Informe o nocional em US$.");
        RuleFor(x => x.OptionKind).NotNull().IsInEnum().When(x => x.Type == OrderType.Option);
        RuleFor(x => x.Premium).NotNull().GreaterThanOrEqualTo(0).When(x => x.Type == OrderType.Option);
        RuleFor(x => x.Notes).MaximumLength(DomainGuard.DescriptionMaxLength);
    }
}

