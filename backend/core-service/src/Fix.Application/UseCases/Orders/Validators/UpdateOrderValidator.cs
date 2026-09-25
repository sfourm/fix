using Fix.Application.Orders.Commands;
using FluentValidation;

namespace Fix.Application.Orders.Validators;

internal sealed class UpdateOrderValidator : AbstractValidator<UpdateOrderCommand>
{
    public UpdateOrderValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.CounterpartyId).NotEmpty();
        RuleFor(x => x.Terms).NotNull().SetValidator(new OrderTermsValidator());
    }
}
