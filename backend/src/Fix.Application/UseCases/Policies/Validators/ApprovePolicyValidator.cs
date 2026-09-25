using Fix.Application.Policies.Commands;
using FluentValidation;

namespace Fix.Application.Policies.Validators;

internal sealed class ApprovePolicyValidator : AbstractValidator<ApprovePolicyCommand>
{
    public ApprovePolicyValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ApprovalRecord).NotEmpty().MaximumLength(100)
            .WithMessage("Informe o número/data da ata do Conselho.");
    }
}
