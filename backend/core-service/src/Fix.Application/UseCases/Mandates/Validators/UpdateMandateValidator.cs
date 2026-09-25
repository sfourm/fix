using Fix.Application.Mandates.Commands;
using FluentValidation;

namespace Fix.Application.Mandates.Validators;

internal sealed class UpdateMandateValidator : AbstractValidator<UpdateMandateCommand>
{
    public UpdateMandateValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Terms).NotNull().SetValidator(new MandateTermsValidator());
    }
}
