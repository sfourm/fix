using Fix.Application.Orders.Commands;
using FluentValidation;

namespace Fix.Application.Orders.Validators;

internal sealed class RegisterOrderValidator : AbstractValidator<RegisterOrderCommand>
{
    public RegisterOrderValidator()
    {
        RuleFor(x => x.MandateId).NotEmpty().WithMessage("Vincule a boleta a um mandato.");
        RuleFor(x => x.CounterpartyId).NotEmpty();
        RuleFor(x => x.Terms).NotNull().SetValidator(new OrderTermsValidator());
    }
}
