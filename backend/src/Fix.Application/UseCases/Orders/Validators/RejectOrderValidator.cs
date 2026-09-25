using Fix.Application.Orders.Commands;
using FluentValidation;

namespace Fix.Application.Orders.Validators;

internal sealed class RejectOrderValidator : AbstractValidator<RejectOrderCommand>
{
    public RejectOrderValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500).WithMessage("A rejeição exige justificativa.");
    }
}
