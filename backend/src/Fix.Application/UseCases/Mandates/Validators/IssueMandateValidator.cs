using Fix.Application.Mandates.Commands;
using FluentValidation;

namespace Fix.Application.Mandates.Validators;

internal sealed class IssueMandateValidator : AbstractValidator<IssueMandateCommand>
{
    public IssueMandateValidator()
    {
        RuleFor(x => x.PolicyId).NotEmpty();
        RuleFor(x => x.AxisId).NotEmpty();
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Terms).NotNull().SetValidator(new MandateTermsValidator());
    }
}
