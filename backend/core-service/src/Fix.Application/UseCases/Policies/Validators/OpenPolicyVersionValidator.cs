using Fix.Application.Policies.Commands;
using FluentValidation;

namespace Fix.Application.Policies.Validators;

internal sealed class OpenPolicyVersionValidator : AbstractValidator<OpenPolicyVersionCommand>
{
    public OpenPolicyVersionValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Version).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Reason).MaximumLength(300);
    }
}
