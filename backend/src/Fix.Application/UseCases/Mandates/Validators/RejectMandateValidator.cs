using Fix.Application.Mandates.Commands;
using FluentValidation;

namespace Fix.Application.Mandates.Validators;

internal sealed class RejectMandateValidator : AbstractValidator<RejectMandateCommand>
{
    public RejectMandateValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500).WithMessage("A rejeição exige justificativa.");
    }
}
